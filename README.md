# Validation Library

This repository provides infrastructure components for configuring validation flows using MassTransit. Flows are defined using JSON configuration files.

## Validation Flow Configuration Schema

Each element in the configuration array has the following properties:

- `Type` (string, required): Fully qualified type name of the entity to validate.
- `SaveValidation` (bool): Registers the `SaveValidationConsumer<T>`.
- `SaveCommit` (bool): Registers the `SaveCommitConsumer<T>`.
- `DeleteValidation` (bool): Registers the `DeleteValidationConsumer<T>`.
- `DeleteCommit` (bool): Registers the `DeleteCommitConsumer<T>`.
- `MetricProperty` (string, optional): Name of the numeric property used for metric based validation.
- `ThresholdType` (int, optional): Specifies the threshold comparison type.
- `ThresholdValue` (decimal, optional): Value used with `ThresholdType`.
- `ManualValidationRules` (array of strings, optional): Names of entity properties used to generate simple manual validator rules. String properties must be non‑empty and numeric properties must be greater than zero.

## Sample Configuration

A sample configuration file is provided in [`config/sample-validation-flows.json`](config/sample-validation-flows.json):

```json
[
  {
    "Type": "Validation.Domain.Entities.Item, Validation.Domain",
    "SaveValidation": true,
    "SaveCommit": true,
    "DeleteValidation": true,
    "DeleteCommit": true,
    "MetricProperty": "Metric",
    "ThresholdType": 1,
    "ThresholdValue": 0.2,
    "ManualValidationRules": ["Metric"]
  }
]
```

Use `AddValidationFlows` to register flows from this configuration.

```csharp
var configs = JsonSerializer.Deserialize<List<ValidationFlowConfig>>(File.ReadAllText("config/sample-validation-flows.json"));
services.AddValidationFlows(configs!);
```
