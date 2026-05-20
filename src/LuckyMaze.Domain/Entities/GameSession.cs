using System.Collections.Concurrent;
using LuckyMaze.Domain.Enums;
using Stateless;

namespace LuckyMaze.Domain.Entities
{
    public class GameSession
    {
        public string SessionId { get; } = Guid.NewGuid().ToString();
        public ConcurrentDictionary<string, PlayerState> Players { get; } = new();
        
        private readonly StateMachine<GameState, GameTrigger> _stateMachine;
        
        // Expose current state
        public GameState CurrentState => _stateMachine.State;
        
        // Events
        public event Action? OnMazeGenerated;
        public event Action<GameState>? OnStateChanged;

        public GameSession()
        {
            _stateMachine = new StateMachine<GameState, GameTrigger>(GameState.WaitingForPlayers);

            _stateMachine.OnTransitionCompleted(t => OnStateChanged?.Invoke(t.Destination));

            _stateMachine.Configure(GameState.WaitingForPlayers)
                .PermitIf(GameTrigger.PlayerReady, GameState.BettingPhase, CanStartBettingPhase);

            _stateMachine.Configure(GameState.BettingPhase)
                .OnEntry(() => OnMazeGenerated?.Invoke())
                .PermitIf(GameTrigger.BetPlaced, GameState.GameRunning, CanStartGameRunning);

            _stateMachine.Configure(GameState.GameRunning)
                .Permit(GameTrigger.AiReachedExit, GameState.GameFinished);

            _stateMachine.Configure(GameState.GameFinished)
                .Permit(GameTrigger.RestartGame, GameState.WaitingForPlayers);
        }

        public void AddPlayer(string userId)
        {
            Players.TryAdd(userId, new PlayerState { UserId = userId });
        }

        public void SetPlayerReady(string userId)
        {
            if (Players.TryGetValue(userId, out var player))
            {
                player.IsReady = true;
                if (_stateMachine.CanFire(GameTrigger.PlayerReady))
                {
                    _stateMachine.Fire(GameTrigger.PlayerReady);
                }
            }
        }

        public void PlaceBet(string userId, string exitId)
        {
            if (CurrentState != GameState.BettingPhase)
                return;

            if (Players.TryGetValue(userId, out var player))
            {
                player.BetExitId = exitId;
                if (_stateMachine.CanFire(GameTrigger.BetPlaced))
                {
                    _stateMachine.Fire(GameTrigger.BetPlaced);
                }
            }
        }

        public void FinishGame()
        {
            if (_stateMachine.CanFire(GameTrigger.AiReachedExit))
            {
                _stateMachine.Fire(GameTrigger.AiReachedExit);
            }
        }

        private bool CanStartBettingPhase()
        {
            if (Players.Count < 2) return false;
            
            var readyCount = Players.Values.Count(p => p.IsReady);
            var threshold = Math.Ceiling(Players.Count * 0.7);
            
            return readyCount >= threshold;
        }

        private bool CanStartGameRunning()
        {
            // All players must have placed a bet
            return Players.Count > 0 && Players.Values.All(p => p.HasBet);
        }
    }
}
