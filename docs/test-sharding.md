# Smart Test Sharding

## Overview
The framework implements a smart test sharding strategy that distributes tests across multiple shards based on their historical execution times rather than just the number of tests. This ensures more balanced parallel execution by considering the actual duration of each test.

## How It Works

### 1. Test Duration Tracking
- The `TestMetricsManager` automatically records execution metrics for each test run:
  - Test duration
  - Pass/fail status
  - Start and end times
  - Test categories
  - Error information (if any)

### 2. Smart Distribution Algorithm
The `ShardingStrategy` class implements the following logic:

```csharp
// Example: If you have 40 tests and 5 shards
TEST_TOTAL_SHARDS=5
TEST_SHARD_INDEX=0..4 (one for each shard)
```

#### Distribution Process:
1. **Weight Calculation**:
   - Uses historical test execution times as weights
   - New tests get a default weight of 1.0 second
   - Only successful test runs are considered for accuracy

2. **Test Assignment**:
   - Tests are sorted by duration (descending)
   - Each test is assigned to the shard with the lowest current total weight
   - This ensures long-running tests are evenly distributed

### 3. Implementation Details

#### Environment Variables
- `TEST_SHARD_INDEX`: Current shard number (0-based)
- `TEST_TOTAL_SHARDS`: Total number of shards

#### Key Components:
1. **TestMetricsManager**:
   - Records and maintains test execution history
   - Provides historical data for sharding decisions

2. **ShardingStrategy**:
   - Determines which shard should run each test
   - Implements the smart distribution algorithm

3. **TestBase Integration**:
   - Checks shard assignment in test setup
   - Skips tests not assigned to current shard

## Usage

### Azure Pipelines Configuration
```yaml
variables:
  TOTAL_SHARDS: 5

jobs:
- job: TestShard
  strategy:
    matrix:
      Shard0:
        TEST_SHARD_INDEX: 0
      Shard1:
        TEST_SHARD_INDEX: 1
      Shard2:
        TEST_SHARD_INDEX: 2
      Shard3:
        TEST_SHARD_INDEX: 3
      Shard4:
        TEST_SHARD_INDEX: 4
  steps:
  - script: dotnet test
```

### Benefits
1. **Balanced Execution Time**:
   - Instead of just splitting by test count (e.g., 8 tests per shard)
   - Considers actual test duration for better distribution

2. **Adaptive Distribution**:
   - Automatically adjusts as test execution times change
   - Handles new tests gracefully with default weights

3. **Improved Parallelization**:
   - More efficient resource utilization
   - Reduced overall execution time
   - Better load balancing across CI/CD runners

### Example Scenarios

#### Scenario 1: Mixed Test Durations
```
Tests:
- Test A: 5 minutes
- Test B: 4 minutes
- Test C: 3 minutes
- Test D: 1 minute
- Test E: 30 seconds

With 2 shards:
Shard 0: Test A, Test D (6 minutes)
Shard 1: Test B, Test C, Test E (7.5 minutes)
```

#### Scenario 2: New Tests
```
When a new test is added:
1. Assigned default weight (1 second)
2. After first run, uses actual execution time
3. Distribution adjusts in subsequent runs
```

## Maintenance

### Best Practices
1. Regularly clean up old test metrics (if needed)
2. Monitor shard execution times for balance
3. Adjust shard count based on:
   - Total test count
   - Test duration distribution
   - Available CI/CD resources

### Troubleshooting
1. **Uneven Distribution**:
   - Check test execution history
   - Verify metrics are being recorded
   - Consider increasing shard count

2. **New Tests**:
   - Will use default weight first run
   - Distribution improves after execution history exists 