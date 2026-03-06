# Power Supply Integration Tests

This project contains integration tests for the OneDriver PowerSupply solution.

## ✅ ALL TESTS USE MOCKING - NO HARDWARE REQUIRED

**All integration tests in this project use Moq to mock the HAL layer**, which means:
- ✅ No physical power supply device is required
- ✅ Tests run fast and reliably in CI/CD
- ✅ All tests can run on any development machine
- ✅ Perfect for automated testing pipelines

## Test Organization

### MockDeviceHelper.cs
**Shared helper class** that creates mocked devices with standard configurations:
- Creates `Mock<IPowerSupplyHal>` with typical Kd3005p specifications (30V, 5A)
- Configures all HAL methods to return success
- Simplifies test setup across all test files

### 1. MockedDeviceIntegrationTests.cs (18 tests)
**Purpose:** HAL integration and method verification

- Tests Device → HAL integration
- **Verifies** that parameter changes correctly call HAL methods (using `Verify()`)
- Tests multi-channel scenarios
- Custom HAL specifications (different voltage/current limits)

### 2. PowerSupplyFactoryIntegrationTests.cs (16 tests)
**Purpose:** Device creation and initialization

- Tests device specifications after creation
- Verifies channel initialization
- Parameter validation (min/max limits)
- All device operations (On/Off, SetVolts, SetAmps)

### 3. DeviceLifecycleIntegrationTests.cs (12 tests)
**Purpose:** Device lifecycle and state management

- Device state through configuration
- Parameter persistence
- Boundary value testing
- Control mode switching
- Multiple independent device instances

### 4. CrossComponentIntegrationTests.cs (14 tests)
**Purpose:** Cross-component integration

- Device → Channel → Parameter integration
- Type system validation (inheritance, interfaces)
- Observable collections behavior
- Sequential parameter changes
- Boundary testing

## Running Tests

### All Tests (Recommended)
```bash
dotnet test
```

All tests use mocking, so they will run successfully without any hardware!

### Run Specific Test Class
```bash
dotnet test --filter "FullyQualifiedName~MockedDeviceIntegrationTests"
dotnet test --filter "FullyQualifiedName~PowerSupplyFactoryIntegrationTests"
```

## Benefits of This Approach

1. **✅ No Hardware Dependencies** - All tests use mocks
2. **✅ Fast Execution** - No serial port communication delays
3. **✅ CI/CD Ready** - Runs in GitHub Actions, Azure Pipelines, etc.
4. **✅ Reliable** - No flaky tests due to hardware issues
5. **✅ Developer Friendly** - Run tests anywhere, anytime
6. **✅ Comprehensive Coverage** - Tests all integration scenarios

## Total Test Coverage

- **60 integration tests** across 4 test files
- All using mocked HAL layer
- No physical hardware required
- Perfect for continuous integration

