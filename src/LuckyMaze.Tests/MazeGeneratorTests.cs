using System;
using System.Collections.Generic;
using System.Text.Json;
using LuckyMaze.Application.Services;
using LuckyMaze.Domain;
using TUnit.Assertions;

namespace LuckyMaze.Tests
{
    public class MazeGeneratorTests
    {
        private readonly MazeGenerator _generator = new();

        [Test]
        public async Task Generate_ValidDimensions_ReturnsMaze()
        {
            // Act
            var maze = _generator.Generate(5, 5, 2);

            // Assert
            await Assert.That(maze).IsNotNull();
            await Assert.That(maze.Width).IsEqualTo(5);
            await Assert.That(maze.Height).IsEqualTo(5);

            var cells = JsonSerializer.Deserialize<List<MazeCell>>(maze.GridData);
            await Assert.That(cells).IsNotNull();
            await Assert.That(cells!.Count).IsEqualTo(25);

            var exits = JsonSerializer.Deserialize<List<MazeExit>>(maze.Exits);
            await Assert.That(exits).IsNotNull();
            await Assert.That(exits!.Count).IsEqualTo(2);
        }

        [Test]
        public async Task Generate_InvalidDimensions_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
            {
                _generator.Generate(2, 5, 2);
                return Task.CompletedTask;
            });
        }

        [Test]
        public async Task Generate_InvalidExitCount_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
            {
                _generator.Generate(5, 5, 1);
                return Task.CompletedTask;
            });
        }

        [Test]
        public async Task Generate_ExitsAreOnBorders()
        {
            // Arrange
            int width = 7;
            int height = 7;

            // Act
            var maze = _generator.Generate(width, height, 2);
            var exits = JsonSerializer.Deserialize<List<MazeExit>>(maze.Exits)!;

            // Assert
            foreach (var exit in exits)
            {
                bool isOnBorder = exit.X == 0 || exit.X == width - 1 || exit.Y == 0 || exit.Y == height - 1;
                await Assert.That(isOnBorder).IsTrue();
            }
        }
    }
}
