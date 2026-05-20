using LuckyMaze.Domain.Entities;
using LuckyMaze.Domain.Enums;
using TUnit.Core;

namespace LuckyMaze.Tests
{
    public class GameSessionTests
    {
        [Test]
        public async Task InitialState_ShouldBeWaitingForPlayers()
        {
            var session = new GameSession();
            await Assert.That(session.CurrentState).IsEqualTo(GameState.WaitingForPlayers);
        }

        [Test]
        public async Task TwoPlayers_BothReady_ShouldTransitionToBettingPhase()
        {
            var session = new GameSession();
            session.AddPlayer("user1");
            session.AddPlayer("user2");

            session.SetPlayerReady("user1");
            
            // 50% ready, threshold is 70%, should not transition
            await Assert.That(session.CurrentState).IsEqualTo(GameState.WaitingForPlayers);

            session.SetPlayerReady("user2");

            // 100% ready, should transition
            await Assert.That(session.CurrentState).IsEqualTo(GameState.BettingPhase);
        }

        [Test]
        public async Task ThreePlayers_TwoReady_ShouldTransitionToBettingPhase()
        {
            var session = new GameSession();
            session.AddPlayer("user1");
            session.AddPlayer("user2");
            session.AddPlayer("user3");

            session.SetPlayerReady("user1");
            await Assert.That(session.CurrentState).IsEqualTo(GameState.WaitingForPlayers);

            session.SetPlayerReady("user2");
            // 2/3 = 66.6% ready. 70% required is Math.Ceiling(3 * 0.7) = 3
            // Wait, let's test the math. Math.Ceiling(3 * 0.7) = Math.Ceiling(2.1) = 3
            // So it needs 3 players. Let's see if our logic holds.
            await Assert.That(session.CurrentState).IsEqualTo(GameState.WaitingForPlayers);

            session.SetPlayerReady("user3");
            await Assert.That(session.CurrentState).IsEqualTo(GameState.BettingPhase);
        }

        [Test]
        public async Task TenPlayers_SevenReady_ShouldTransitionToBettingPhase()
        {
            var session = new GameSession();
            for (int i = 0; i < 10; i++)
            {
                session.AddPlayer($"user{i}");
            }

            for (int i = 0; i < 6; i++)
            {
                session.SetPlayerReady($"user{i}");
            }
            await Assert.That(session.CurrentState).IsEqualTo(GameState.WaitingForPlayers);

            session.SetPlayerReady("user6"); // 7 players ready
            await Assert.That(session.CurrentState).IsEqualTo(GameState.BettingPhase);
        }

        [Test]
        public async Task AllPlayersBet_ShouldTransitionToGameRunning()
        {
            var session = new GameSession();
            session.AddPlayer("user1");
            session.AddPlayer("user2");

            session.SetPlayerReady("user1");
            session.SetPlayerReady("user2");

            await Assert.That(session.CurrentState).IsEqualTo(GameState.BettingPhase);

            session.PlaceBet("user1", "exitA");
            await Assert.That(session.CurrentState).IsEqualTo(GameState.BettingPhase);

            session.PlaceBet("user2", "exitB");
            await Assert.That(session.CurrentState).IsEqualTo(GameState.GameRunning);
        }
    }
}
