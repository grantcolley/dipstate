# dipstate
Dipstate provides a simple mechanism to maintain state for an activity based workflow.

### Features
  * Support for async and synchronous execution
  * Action delegates run with context for entry, exit, reset and status changed events
  * Conditional transitioning between states
  * States can have sub-states
  * Support for auto states
  * Support for dependency states
  
### Example Code Snippet for Setting up State

```C#
            var state = new State(1, "My State", 
                        context: myContext,
                        initialiseWithParent: true, 
                        canCompleteParent: false,
                        type: StateType.Standard, 
                        status: StateStatus.Uninitialise)
                .AddActionAsync(StateActionType.Entry, entryActionAsync)
                .AddActionAsync(StateActionType.Status, statusChangedActionAsync)
                .AddActionAsync(StateActionType.Exit, exitActionAsync)
                .AddActionAsync(StateActionType.Reset, resetActionAsync)
                .AddCanCompletePredicateAsync(canCompletePredicateAsync)
                .AddDependency(dependencyState)
                .AddDependant(dependantState)                
                .AddTransition(transitionState)
                .AddSubState(subState);
                
            await state.ExecuteAsync(StateStatus.Initialise);
```

  * **InitialiseWithParent** - indicates the state will be initialised after its parent has been initialised.
  * **CanCompleteParent** - indicates the state will attempt to complete its parent state once it has completed itself. This will typically be the last state in a workflow which will its parent up to and including the root workflow state.
  * **Type**
    * **Root** is reserved for the main root state in a workflow. There can be only one root within a workflow.
    * **Auto** states will automatically transition or complete itself after it has been initialised.
    * **Standard** is a plain vanilla state.
  * **Status**
    * UnInitialise
    * Initialise
    * InProgress
    * Complete
    * Fail
  * **Actions**
    * **Entry** actions execute on initialising a state.
    * **StatusChanged** actions execute when the status changes.
    * **Exit** actions execute when the state is completed.
    * **Reset** actions execute when the state is reset to *Uninitialised*.
  * **Dependencies** are one or more states which need to be completed before the state can be initialised.
  * **Dependents** are one or more states which are dependent on the state being completed before they can be initialised themselves. Dependent states can optionally be initialised when the state has completed.
  * **Transitions** are one or more states that the state can transition to after it has completed.
  * **Sub States** are one or more states for wich the state acts as a parent. Sub states can behave like a mini workflow where the parent implicitly assumes the role of the root and the last substate will typically have *CanCompleteParent* set to true.

## How it Works

Here is how it works by way of an example workflow. Note: for a full listing of the code see test class **GitHubReadMeExampleTest.cs** in the test project **DevelopmentInProgress.DipState.Test**.

The example workflow follows the activities of a customer remediation process. The process starts with sending out a letter to a customer informing them a remediation is due and requesting a response. In parallel, data pertaining to the redress is gathered and the amount to be redressed is calculated. If necessary an adjustment is made to the calculated amount. Once the response is received from the customer the case is sent for final review. If the review fails the case is sent back to be re-calculated. If the review passes then payment is made to the customer.

![Alt text](/README-images/Dipstate-example-workflow.png?raw=true "Example workflow")

#### Workflow Setup

```C#
            // Create the states
            
            var remediationWorkflowRoot = new State(100, "Remediation Workflow", 
                type: StateType.Root);

            var communication = new State(200, "Communication", 
                                                initialiseWithParent: true);

            var letterSent = new State(210, "Letter Sent", initialiseWithParent: true)
                .AddActionAsync(StateActionType.Entry, GenerateLetterAsync)
                .AddActionAsync(StateActionType.Status, SaveStatusAsync)
                .AddActionAsync(StateActionType.Exit, NotifyDispatchAsync)
                .AddCanCompletePredicateAsync(LetterChecked);

            var response = new State(220, "Response Received", canCompleteParent: true)
                .AddActionAsync(StateActionType.Status, SaveStatusAsync);
            
            var collateData = new State(300, "Collate Data", true)
                .AddCanCompletePredicateAsync(ValidateData);
            
            var adjustmentDecision = new State(400, "Adjustment Decision", 
                type: StateType.Auto);

            var adjustment = new State(500, "Adjustment");

            var autoTransitionToRedressReview 
                = new State(600, "Auto Transition To Redress Review", 
                                    type: StateType.Auto);

            var redressReview = new State(700, "Redress Review");

            var payment = new State(800, "Payment", canCompleteParent: true);

            // Assemble the workflow
            
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

            remediationWorkflowRoot
                .AddSubState(communication)
                .AddSubState(collateData)
                .AddSubState(adjustmentDecision)
                .AddSubState(adjustment)
                .AddSubState(autoTransitionToRedressReview)
                .AddSubState(redressReview)
                .AddSubState(payment);

            // Initialise the workflow
            
            await remediationWorkflowRoot.ExecuteAsync(StateStatus.Initialise);
```

#### Initialising a State
  * A state cannot initialise if it has one or more **dependency states** that have not yet completed.
  * **Entry actions** are executed with context. 
  * **StatusChanged actions** are executed with context. 
  * If the type is **StateType.Auto** the state will automatically transition. An entry action can determine at runtime which state to transition to. Alternatively, if no transition state has been set, the state will complete itself.  
  * If the state has sub states then those sub states where **InitialiseWithParent** is true will also be initialised. 

The following shows how the initialising the *Remediation Workflow Root* will also initialise *Collate Data*, *Communication* and its sub state *Letter Sent*.

```C#
            await remediationWorkflowRoot.ExecuteAsync(StateStatus.Initialise);
```

![Alt text](/README-images/Dipstate-example-initialiseState.png?raw=true "Initialising a state")

#### Transition a State
  * Transitioning to another state completes the state being transition from.
  * If the state has only one transition state then it will transition to that state when it is set to complete.
  * A delegate is executed to determine whether the state can complete. If no delegate has been provided it will return true.
  * **Exit actions** are executed with context. 
  * **Status actions** are executed with context after the status has changed. 
  * Any dependant states with **InitialiseDependantWhenComplete** set to true will be initialised.
  * The transition state is initialised

The following shows *Collate Data* transition to *AdjustmentDecision*.

```C#
            var adjustmentDecision = await collateData.ExecuteAsync(adjustmentDecision);
```

![Alt text](/README-images/Dipstate-example-transition.png?raw=true "Transition a state")


#### Auto States
Auto states will automatically transition or complete itself after it has been initialised.
**Entry actions** are delegates that enable processing with state context to take place. In the case of an auto state an **Entry action** can be used to determine at runtime which state to transition to.

The following shows the configuration of auto state *AdjustmentDecision* which enables it to transition to either the *Adjustment* or *AutoTransitionToRedressReview* (which happens to be another auto state).

```C#
            adjustmentDecision
                .AddTransition(adjustment)
                .AddTransition(autoTransitionToRedressReview)
                .AddAction(StateActionType.Entry, (s =>
                {
                    // Determine at runtime whether to transition 
                    // to Adjustment or AutoTransitionToReview
                }));
```

![Alt text](/README-images/Dipstate-example-autostate.png?raw=true "Auto state")
