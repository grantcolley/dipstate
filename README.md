# dipstate
Dipstate provides a simple mechanism to maintain state for activities that are transitioned up and down a state based workflow.

## Code Snippet 
```C#
            var response = new State(220, "Response Received", canCompleteParent: true)
                .AddActionAsync(StateActionType.Status, SaveStatusAsync);

            var letterSent = new State(210, "Letter Sent", initialiseWithParent: true)
                .AddActionAsync(StateActionType.Entry, GenerateLetterAsync)
                .AddActionAsync(StateActionType.Status, SaveStatusAsync)
                .AddActionAsync(StateActionType.Exit, NotifyDispatchAsync)
                .AddCanCompletePredicateAsync(LetterChecked)
                .AddTransition(response);

            var communication = new State(200, "Communication")
                .AddSubState(letterSent)
                .AddSubState(response);

            await communication.ExecuteAsync(StateStatus.Initialise);
```

## Features
  * Support for async and synchronous execution
  * Action delegates run with context for entry, exit, reset and status changed events
  * Conditional transitioning between states
  * States can have sub-states
  * Support for auto states
  * Support for dependency states

## How it Works

#### Example Workflow
![Alt text](/README-images/Dipstate-example-workflow.png?raw=true "Example workflow")

### Workflow Setup
```C#
            var remediationWorkflow = new State(100, "Remediation Workflow", type: StateType.Root);

            var communication = new State(200, "Communication", initialiseWithParent: true);

            var letterSent = new State(210, "Letter Sent", initialiseWithParent: true)
                .AddActionAsync(StateActionType.Entry, GenerateLetterAsync)
                .AddActionAsync(StateActionType.Status, SaveStatusAsync)
                .AddActionAsync(StateActionType.Exit, NotifyDispatchAsync)
                .AddCanCompletePredicateAsync(LetterChecked);

            var response = new State(220, "Response Received", canCompleteParent: true)
                .AddActionAsync(StateActionType.Status, SaveStatusAsync);
            
            var collateData = new State(300, "Collate Data", true);
            
            var adjustmentDecision = new State(400, "Adjustment Decision", type: StateType.Auto);

            var adjustment = new State(500, "Adjustment");

            var autoTransitionToRedressReview 
                = new State(600, "Auto Transition To Redress Review", type: StateType.Auto);

            var redressReview = new State(700, "Redress Review");

            var payment = new State(800, "Payment", canCompleteParent: true);

            redressReview
                .AddTransition(payment)
                .AddTransition(collateData)
                .AddDependency(communication)
                .AddDependency(autoTransitionToRedressReview);

            autoTransitionToRedressReview
                .AddTransition(redressReview);

            adjustment.AddTransition(autoTransitionToRedressReview);

            adjustmentDecision
                .AddTransition(adjustment)
                .AddTransition(autoTransitionToRedressReview)
                .AddAction(StateActionType.Entry, (s =>
                {
                    // Determine at runtime whether to transition 
                    // to Adjustment or AutoTransitionToReview
                }));

            collateData
                .AddTransition(adjustmentDecision);

            letterSent.AddTransition(response);

            communication.AddDependant(redressReview, true)
                .AddSubState(letterSent)
                .AddSubState(response);

            remediationWorkflow
                .AddSubState(communication)
                .AddSubState(collateData)
                .AddSubState(adjustmentDecision)
                .AddSubState(adjustment)
                .AddSubState(autoTransitionToRedressReview)
                .AddSubState(redressReview)
                .AddSubState(payment);

            await remediationWorkflow.ExecuteAsync(StateStatus.Initialise);
```
