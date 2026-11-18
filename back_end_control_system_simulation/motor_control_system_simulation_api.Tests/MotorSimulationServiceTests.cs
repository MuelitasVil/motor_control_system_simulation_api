using MotorControlSystemSimulationApi.Models.Entities;
using MotorControlSystemSimulationApi.Models.Enums;
using MotorControlSystemSimulationApi.Repositories.Interfaces;
using MotorControlSystemSimulationApi.Services;
using Moq;
using Xunit;

namespace MotorControlSystemSimulationApi.Tests;

public sealed class MotorSimulationServiceTests
{
    [Fact]
    public async Task UpdateSpeedAsync_WhenRequestExceedsDrivingModeLimit_AppliesModeLimit()
    {
        var fixture = CreateFixture(currentSpeed: 40m, batteryLevel: 85m);

        var result = await fixture.Service.UpdateSpeedAsync(
            fixture.AssociatedMotor.AssociatedMotorId,
            new UpdateSpeedRequest(170m, DrivingMode.Normal));

        Assert.NotNull(result);
        Assert.Equal(144m, result.AppliedSpeed); // 80 % de la velocidad máxima (180 km/h).
        Assert.Equal(144m, fixture.AssociatedMotor.CurrentSpeed);
        fixture.AssociatedMotorRepository.Verify(
            repository => repository.UpdateAsync(
                fixture.AssociatedMotor,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateSpeedAsync_WhenRequestedSpeedIsNegative_ThrowsAndDoesNotUpdateRepositories()
    {
        var fixture = CreateFixture(currentSpeed: 40m, batteryLevel: 85m);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            fixture.Service.UpdateSpeedAsync(
                fixture.AssociatedMotor.AssociatedMotorId,
                new UpdateSpeedRequest(-5m, DrivingMode.Normal)));

        fixture.AssociatedMotorRepository.Verify(
            repository => repository.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        fixture.StateHistoryRepository.Verify(
            repository => repository.AddAsync(
                It.IsAny<MotorStateHistory>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateSpeedAsync_WhenBatteryRunsOut_ReducesSpeedToZeroOverUpdates()
    {
        // 0.01 % no alcanza para el consumo estimado del primer ciclo.
        var fixture = CreateFixture(currentSpeed: 35m, batteryLevel: 0.01m);
        var appliedSpeeds = new List<decimal>();

        // Cada actualización representa un ciclo de simulación; al agotarse la batería,
        // el vehículo reduce 10 km/h por ciclo hasta detenerse.
        for (var cycle = 0; cycle < 4; cycle++)
        {
            var result = await fixture.Service.UpdateSpeedAsync(
                fixture.AssociatedMotor.AssociatedMotorId,
                new UpdateSpeedRequest(120m, DrivingMode.Sport));

            Assert.NotNull(result);
            appliedSpeeds.Add(result.AppliedSpeed);
        }

        Assert.Equal(new[] { 25m, 15m, 5m, 0m }, appliedSpeeds);
        Assert.Equal(0m, fixture.AssociatedMotor.BatteryLevel);
        Assert.Equal(0m, fixture.AssociatedMotor.CurrentSpeed);
        Assert.Equal(0, fixture.AssociatedMotor.CurrentRpm);
        Assert.Equal(0m, fixture.AssociatedMotor.CurrentPower);
    }

    [Fact]
    public async Task UpdateSpeedAsync_WhenMotorIsStopped_RejectsSpeedChange()
    {
        var fixture = CreateFixture(currentSpeed: 0m, batteryLevel: 100m);
        fixture.AssociatedMotor.OperationState = MotorOperationState.Stopped;

        await Assert.ThrowsAsync<MotorNotInTripException>(() =>
            fixture.Service.UpdateSpeedAsync(
                fixture.AssociatedMotor.AssociatedMotorId,
                new UpdateSpeedRequest(40m, DrivingMode.Normal)));
    }

    private static TestFixture CreateFixture(decimal currentSpeed, decimal batteryLevel)
    {
        var motor = new Motor
        {
            MotorId = Guid.NewGuid(),
            Model = "Mock motor",
            MaximumSpeed = 180m,
            MaximumTemperature = 150m,
            BatteryCapacityKwh = 75m,
            MaximumPower = 220m
        };

        var associatedMotor = new MotorAssociated
        {
            AssociatedMotorId = Guid.NewGuid(),
            MotorId = motor.MotorId,
            Motor = motor,
            CurrentSpeed = currentSpeed,
            BatteryLevel = batteryLevel,
            CurrentTemperature = 30m,
            CurrentRpm = 1000,
            CurrentPower = 20m,
            DrivingMode = DrivingMode.Normal,
            OperationState = MotorOperationState.InTrip
        };

        var associatedMotorRepository = new Mock<IEntityRepository<MotorAssociated>>();
        associatedMotorRepository
            .Setup(repository => repository.GetByIdAsync(
                associatedMotor.AssociatedMotorId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(associatedMotor);
        associatedMotorRepository
            .Setup(repository => repository.UpdateAsync(
                It.IsAny<MotorAssociated>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var stateHistoryRepository = new Mock<IEntityRepository<MotorStateHistory>>();
        stateHistoryRepository
            .Setup(repository => repository.AddAsync(
                It.IsAny<MotorStateHistory>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var commandRepository = new Mock<IEntityRepository<Command>>();
        commandRepository
            .Setup(repository => repository.AddAsync(
                It.IsAny<Command>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new MotorSimulationService(
            associatedMotorRepository.Object,
            stateHistoryRepository.Object,
            commandRepository.Object);

        return new TestFixture(
            service,
            associatedMotor,
            associatedMotorRepository,
            stateHistoryRepository);
    }

    private sealed record TestFixture(
        MotorSimulationService Service,
        MotorAssociated AssociatedMotor,
        Mock<IEntityRepository<MotorAssociated>> AssociatedMotorRepository,
        Mock<IEntityRepository<MotorStateHistory>> StateHistoryRepository);
}
