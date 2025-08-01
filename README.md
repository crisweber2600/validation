# validation

## Validation Flow Configuration

`AddValidationFlows` reads a collection of `ValidationFlowConfig` objects, which can be loaded from JSON.
Each entry specifies which consumers to enable and optional validation settings.

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
    "ManualRules": ["Metric > 0"]
  }
]
```

* `Type` – fully qualified type of the entity.
* `SaveValidation` / `SaveCommit` – registers save validation and commit consumers.
* `DeleteValidation` / `DeleteCommit` – registers delete validation and commit consumers.
* `MetricProperty`, `ThresholdType`, `ThresholdValue` – configure automatic validation plans.
* `ManualRules` – list of simple rule expressions (`Property operator value`) registered via `AddValidatorRule`.

Sample configurations are provided under the `config` folder.
