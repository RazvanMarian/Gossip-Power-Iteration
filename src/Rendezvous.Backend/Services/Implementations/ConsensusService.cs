using Rendezvous.Model;
using Rendezvous.Services.Abstractions;

namespace Rendezvous.Services.Implementations;

public class ConsensusService : BaseBackgroundService
{
    private readonly int _maxIterations;
    private readonly TimeSpan _waitTimeout;

    public ConsensusService(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ConsensusState state,
        ILogger<ConsensusService> logger
        ) : base(httpClientFactory, config, state, logger) 
    {
        _maxIterations = _config.GetValue("MaxIterations", 1000);
        _waitTimeout = TimeSpan.FromSeconds(_config.GetValue<double>("WaitTimeoutSeconds", 10));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            InitializeConsensusState();
            await SendInitialStateAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested && _state.IterationsCompleted < _maxIterations - 1)
            {
                int iterationToWaitFor = _state.IterationsCompleted + 1;

                bool dataReady = WaitForIterationData(iterationToWaitFor, stoppingToken);
                if (!dataReady) break; 

                bool updateSucceeded = PerformIterationUpdateAndFinalize(iterationToWaitFor);
                if (!updateSucceeded) break; 

                await SendUpdateForNextIterationAsync(iterationToWaitFor, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("[{NodeId}] Consensus process canceled.", _nodeId);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[{NodeId}] FATAL ERROR in ConsensusService ExecuteAsync.", _nodeId);
        }
        finally
        {
            _logger.LogInformation("[{NodeId}] ConsensusService execution loop finished. Completed Iterations: {CompletedCount}. Final value: {FinalValue:F4}",
                _nodeId, _state.IterationsCompleted + 1, _state.Value);
        }
    }

    private void InitializeConsensusState()
    {
        lock (_state)
        {
            _state.IterationsCompleted = -1;
            _state.IncomingValuesPerIteration.Clear();
        }
    }

    private async Task SendInitialStateAsync(CancellationToken stoppingToken)
    {
        await SendToNeighboursInternalAsync(0, stoppingToken);
    }

    private bool WaitForIterationData(int iterationToWaitFor, CancellationToken stoppingToken)
    {
        int expectedMessages = 0;
        bool dataReady = false;
        bool alreadyLoggedWait = false; 

        while (!dataReady && !stoppingToken.IsCancellationRequested)
        {
            lock (_state)
            {
                expectedMessages = _state.IncomingWeightsMap.Count;
                dataReady = _state.IncomingValuesPerIteration.TryGetValue(iterationToWaitFor, out var currentIterationData)
                            && currentIterationData.Count == expectedMessages;

                if (dataReady) break; 

                if (!alreadyLoggedWait)
                {
                    int currentCount = _state.IncomingValuesPerIteration.GetValueOrDefault(iterationToWaitFor)?.Count ?? 0;
                    alreadyLoggedWait = true; 
                }

                bool pulsed = Monitor.Wait(_state, _waitTimeout);

                if (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("[{NodeId}] Cancellation requested during wait for iteration {Iteration}.", _nodeId, iterationToWaitFor);
                    return false; 
                }

                dataReady = _state.IncomingValuesPerIteration.TryGetValue(iterationToWaitFor, out currentIterationData)
                            && currentIterationData.Count == expectedMessages;

                if (!pulsed && !dataReady) 
                {
                    int currentCountAfterTimeout = _state.IncomingValuesPerIteration.GetValueOrDefault(iterationToWaitFor)?.Count ?? 0;
                    _logger.LogWarning("[{NodeId}] Timeout waiting for iteration {Iteration}. Have {CurrentCount}/{ExpectedMessages}.",
                        _nodeId, iterationToWaitFor, currentCountAfterTimeout, expectedMessages);
                    
                    alreadyLoggedWait = false; 
                }
                else if (pulsed && !dataReady) 
                {
                    int currentCountAfterPulse = _state.IncomingValuesPerIteration.GetValueOrDefault(iterationToWaitFor)?.Count ?? 0;
                       
                    alreadyLoggedWait = false; 
                }
            } 
        } 

        return dataReady && !stoppingToken.IsCancellationRequested;
    }

    private bool PerformIterationUpdateAndFinalize(int iterationCompleted)
    {
        lock (_state)
        {
            if (!_state.IncomingValuesPerIteration.TryGetValue(iterationCompleted, out var iterationData))
            {
                _logger.LogError("[{NodeId}] Error: Update logic entered for iteration {Iteration} but data not found!", _nodeId, iterationCompleted);
                return false;
            }

            double receivedWeightedSum = iterationData.Values.Sum();
            double currentValue = _state.Value;
            double selfTerm = currentValue * _state.SelfWeight;
            double newValue = receivedWeightedSum + selfTerm;

            Console.WriteLine($"Update Iteration:{iterationCompleted} | OldVal:{currentValue:F4} | NewVal:{newValue:F4}");
            //_logger.LogInformation("[{NodeId}] Update Iteration:{Iteration} | OldVal:{OldValue:F4} | RecvSum:{ReceivedSum:F4} | SelfTerm:{SelfTerm:F4} | NewVal:{NewValue:F4}",
            //   _nodeId, iterationCompleted, currentValue, receivedWeightedSum, selfTerm, newValue);

            _state.Value = newValue;

            _state.IterationsCompleted = iterationCompleted;

            _state.IncomingValuesPerIteration.Remove(iterationCompleted);
            
            return true;
        } 
    }

    private async Task SendUpdateForNextIterationAsync(int iterationCompleted, CancellationToken stoppingToken)
    {
        int iterationToSend = iterationCompleted + 1;
        if (iterationToSend < _maxIterations)
        {
            await SendToNeighboursInternalAsync(iterationToSend, stoppingToken);
        }
        else
        {
            _logger.LogInformation("[{NodeId}] Reached MaxIterations ({MaxIterations}). Not sending for iteration {Iteration}.", _nodeId, _maxIterations, iterationToSend);
        }
    }

    private async Task SendToNeighboursInternalAsync(int iteration, CancellationToken stoppingToken)
    {
        double valueToSend;

        lock (_state) { valueToSend = _state.Value; }

        var messagePayload = new ConsensusMessage(_nodeId, valueToSend, iteration, DateTime.UtcNow);
        int peerCount = _outgoingPeerUrls.Count;

        var sendTasks = new List<Task>();
        foreach (var peerUrl in _outgoingPeerUrls)
        {
            if (stoppingToken.IsCancellationRequested) break;
            sendTasks.Add(SendSingleMessageAsync(peerUrl, iteration, messagePayload, stoppingToken));
        }
        await Task.WhenAll(sendTasks);
    }


    private async Task SendSingleMessageAsync(string peerUrl, int iteration, ConsensusMessage messagePayload, CancellationToken stoppingToken)
    {
        try
        {
            var url = $"{peerUrl}/consensus";
            var response = await _client.PostAsJsonAsync(url, messagePayload, stoppingToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[{NodeId}] Error sending Iteration {Iteration} to {PeerUrl}. Status: {StatusCode}",
                   _nodeId, iteration, peerUrl, response.StatusCode);
            }
        }
        catch (OperationCanceledException) { _logger.LogWarning("[{NodeId}] Sending iteration {Iteration} to {PeerUrl} cancelled.", _nodeId, iteration, peerUrl); }
        catch (HttpRequestException httpEx) { _logger.LogWarning(httpEx, "[{NodeId}] HTTP error sending Iteration {Iteration} to {PeerUrl}.", _nodeId, iteration, peerUrl); }
        catch (Exception ex) { _logger.LogError(ex, "[{NodeId}] Exception sending Iteration {Iteration} to {PeerUrl}.", _nodeId, iteration, peerUrl); }
    }
} 
