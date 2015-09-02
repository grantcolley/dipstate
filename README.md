# dipstate
Dipstate provides a simple mechanism to maintain state for an activity based workflow.

### Features
  * Support for async and synchronous execution
  * Action delegates execute with context for entry, exit, reset and status changed events
  * Conditional transitioning between states
  * States can have sub-states
  * Support for auto states
  * Support for dependency states

## How it works

Here is how it works by way of an example workflow. 

**Note:**
For a full listing of the code see test class [GitHubReadMeExampleTest.cs](https://github.com/grantcolley/dipstate/tree/master/DevelopmentInProgress.DipState.Test/GitHubReadMeExampleTest.cs) in [DevelopmentInProgress.DipState.Test](https://github.com/grantcolley/dipstate/tree/master/DevelopmentInProgress.DipState.Test) project.

The example workflow follows the activities of a customer remediation process. The process starts by sending out a letter to a customer informing them a redress is due on their account and a response is required. While waiting for a response from the customer data pertaining to the redress is gathered and the amount to be redressed is calculated. If necessary an adjustment is made to the calculated amount. After both the redress amount has been calculated and the customer response has been received the case is sent for final review. If the final review fails the case is sent back to be re-calculated. If the review passes then payment is made to the customer.

![Alt text](/README-images/Dipstate-example-workflow.png?raw=true "Example workflow")

You can find an example WPF implementation of the workflow at [Origin](https://github.com/grantcolley/origin)
![Alt text](/README-images/WPF-Example.PNG?raw=true "WPF implementation of the Dipstate example workflow in DevelopmentInProgress.Origin").

#### Setting up the workflow
```C#
            // Create the remediation workflow states

            var remediationWorkflowRoot 
                = new State(100, "Remediation Workflow", StateType.Root);

            var communication = new State(200, "Communication");

            var letterSent = new State(210, "Letter Sent")
                .AddActionAsync(StateActionType.OnEntry, GenerateLetterAsync)
                .AddActionAsync(StateActionType.OnStatusChanged, SaveStatusAsync)
                .AddActionAsync(StateActionType.OnExit, NotifyDispatchAsync)
                .AddCanInitialisePredicateAsync(CanInitialiseLetterSentAsync)
                .AddCanChangeStatusPredicateAsync(CanChangeLetterSentStatusAsync)
                .AddCanCompletePredicateAsync(CanCompleteLetterSentAsync)
                .AddCanResetPredicateAsync(CanResetLetterSentAsync);

            var responseReceived = new State(220, "Response Received");

            var collateData = new State(300, "Collate Data");

            var adjustmentDecision 
                = new State(400, "Adjustment Decision", StateType.Auto);

            var adjustment = new State(500, "Adjustment");

            var autoTransitionToRedressReview
                = new State(600, "Auto Transition To Redress Review", StateType.Auto);

            var redressReview = new State(700, "Redress Review");

            var payment = new State(800, "Payment");


            // Assemble the remediation workflow

            redressReview
                .AddTransition(payment, true)
                .AddTransition(collateData)
                .AddDependency(communication, true)
                .AddDependency(autoTransitionToRedressReview, true)
                .AddActionAsync(StateActionType.OnEntry, CalculateFinalRedressAmountAsync);

            autoTransitionToRedressReview
                .AddDependant(redressReview, true)
                .AddTransition(redressReview, true);

            adjustment.AddTransition(autoTransitionToRedressReview, true);

            adjustmentDecision
                .AddTransition(adjustment)
                .AddTransition(autoTransitionToRedressReview)
                .AddActionAsync(StateActionType.OnEntry, ConditionalTransitionDecisionAsync);

            collateData
                .AddTransition(adjustmentDecision, true);

            letterSent.AddTransition(responseReceived, true);

            communication
                .AddSubState(letterSent, true)
                .AddSubState(responseReceived)
                .AddDependant(redressReview, true)
                .AddTransition(redressReview, true);

            remediationWorkflowRoot
                .AddSubState(communication, true)
                .AddSubState(collateData, true)
                .AddSubState(adjustmentDecision)
                .AddSubState(adjustment, completionRequired: false)
                .AddSubState(autoTransitionToRedressReview)
                .AddSubState(redressReview)
                .AddSubState(payment);
```
##### State Types
  * **Root** is a state that represents a workflow. Its sub states are the states within the workflow. Initialising the root state is the entry point into the workflow and is automatically completed when the last sub state requiring completion has completed.
  * **Auto** is a state which will automatically complete itself after initialisation. Entry actions are executed during the initialisation which is a good place to perform some task or determine the state it needs to transition to at runtime..
  * **Standard** is a plain vanilla state.

The following shows how to create different types of states.
```C#
            var remediationWorkflowRoot 
                = new State(100, "Remediation Workflow", StateType.Root);
                
            // unless otherwise specified a standard state is created
            var collateData = new State(300, "Collate Data");
            
            var adjustmentDecision 
                = new State(400, "Adjustment Decision", StateType.Auto);
```

##### State Properties
  * **InitialiseWithParent** - applies to sub states and indicates it will be initialised when its parent is initialised.
  * **CanCompleteParent** - applies to sub states and indicates the state will attempt to complete its parent state after it has completed.

##### Delegates
  * **Actions**
    * **Entry** action delegates that execute on initialising a state.
    * **StatusChanged** action delegates that execute when the status changes.
    * **Exit** action delegates that execute when the state is completed.
    * **Reset** action delegates that execute when the state is reset to *Uninitialised*.
  * **CanComplete** predicate delegate executed prior to completing a state and transitioning to another one. 
  * **Dependencies** are one or more states which need to be completed before the state can be initialised.
  * **Dependents** are one or more states which are dependent on the state being completed before they can be initialised themselves. Dependent states can optionally be initialised when the state has completed.
  * **Transitions** are one or more states that the state can transition to after it has completed.
  * **Sub States** are one or more states for which the state acts as a parent. Sub states can behave like a mini workflow where the parent implicitly assumes the role of the root and the last substate will typically have CanCompleteParent* set to true.


#### Initialise a State
The following shows how the initialising the *Remediation Workflow Root* will also initialise *Collate Data*, *Communication* and its sub state *Letter Sent*.

```C#
            var result = await remediationWorkflowRoot.ExecuteAsync(StateStatus.Initialise);

            Assert.IsTrue(result.Name.Equals("Remediation Workflow"));
            Assert.AreEqual(remediationWorkflowRoot.Status, StateStatus.Initialise);
            Assert.AreEqual(communication.Status, StateStatus.Initialise);
            Assert.AreEqual(letterSent.Status, StateStatus.Initialise);
            Assert.AreEqual(collateData.Status, StateStatus.Initialise);
```

![Alt text](/README-images/Dipstate-example-initialiseState.png?raw=true "Initialising a state")

Initialising a state
  * A state cannot initialise if it has one or more **dependency states** that have not yet completed.
  * **Entry actions** are executed with context. 
  * **StatusChanged actions** are executed with context. 
  * If the type is **StateType.Auto** the state will automatically transition. An entry action can determine at runtime which state to transition to. Alternatively, if no transition state has been set, the state will complete itself.  
  * If the state has sub states then those sub states where **InitialiseWithParent** is true will also be initialised.


#### Transition a State
The following shows *Letter Sent* transition to *Response*.

```C#
            result = await letterSent.ExecuteAsync(response);
            
            Assert.IsTrue(result.Name.Equals("Response"));
            Assert.AreEqual(letterSent.Status, StateStatus.Complete);
            Assert.AreEqual(response.Status, StateStatus.Initialise);
            Assert.IsTrue(response.Antecedent.Equals(letterSent));
```

![Alt text](/README-images/Dipstate-example-transition.png?raw=true "Transition a state")

Transitioning a state
  * Transitioning from one state to another state will complete the state being transition from and initialise the state being transitioned to.


#### Complete a State
The following shows how *ResponseReceived* is configured to complete itself and its parent, *Communication* which is configured to initialise its dependant state *Redress Review*.

```C#
            var response = new State(220, "Response", canCompleteParent: true)
                .AddActionAsync(StateActionType.Status, SaveStatusAsync);
                
            // ...
            // ...
            // ...
            
            communication
                .AddDependant(redressReview, true)
                .AddSubState(letterSent)
                .AddSubState(response)
                .AddTransition(redressReview);
                
            // ...
            // ...
            // ...
            
            result = await response.ExecuteAsync(StateStatus.Complete);

            Assert.AreEqual(response.Status, StateStatus.Complete);
            Assert.AreEqual(communication.Status, StateStatus.Complete);

            Assert.IsTrue(result.Equals(redressReview));
            Assert.AreEqual(redressReview.Status, StateStatus.Initialise);
            Assert.IsTrue(redressReview.Antecedent.Equals(communication));
```

![Alt text](/README-images/Dipstate-example-substate-close-parent.png?raw=true "Sub state completes its parent")

  * Completing a state
    * First, a delegate is executed to determine whether the state can complete. If no delegate has been provided the state will complete.
    * **Exit actions** are then executed with context before completing the state.
    * The state is set to complete and **Status actions** are executed with context.
    * Any dependant states with **InitialiseDependantWhenComplete** set to true will be initialised.

A sub state can be configured to complete its parent. Typically this will be the last sub state expected to complete under the parent. The last sub state must not be configured to transition to another state so that it can complete its parent. The parent can be configured to transition to another state.

#### Auto States
Auto states will automatically transition or complete itself after it has been initialised.
**Entry actions** are delegates that enable processing with state context to take place. In the case of an auto state an **Entry action** can be used to determine at runtime which state to transition to.

The following shows the configuration of auto state *AdjustmentDecision* which enables it to transition to either the *Adjustment* or *AutoTransitionToRedressReview* (which happens to be another auto state).

```C#
            adjustmentDecision
                .AddTransition(adjustment)
                .AddTransition(autoTransitionToRedressReview)
                .AddActionAsync(StateActionType.Entry, AdjustmentRequiredCheckAsync);
```

![Alt text](/README-images/Dipstate-example-autostate.png?raw=true "Auto state")


#### Dependency States
A state that has one or more dependencies that are not complete cannot be initialised.

Dependency states can be configured to initialise the dependant state when the dependency state completes.

The following shows how *Communication* and *AutoTransitionToRedressReview* are both configured initialise *RedressReview* when they complete. In such a case *RedressReview* will only successfully initialise when the last dependency is completed.

```C#
            autoTransitionToRedressReview
                .AddDependant(redressReview, true)
                .AddTransition(redressReview);
                
            // ...
            // ...
            // ...
            
            communication
                .AddDependant(redressReview, true)
                .AddSubState(letterSent)
                .AddSubState(response)
                .AddTransition(redressReview);
                
```

![Alt text](/README-images/Dipstate-example-dependency.png?raw=true "Dependency States")


#### Failing a State
When a state is failed back to another state, the failed state, the state being failed back to, and all states in between are **reset**. The state being failed back to must be in the transition list and is then *Initialised*.

**Note:** When a state is reset, its status is set to Uninitialised and Reset action delegates will be triggered.

The following shows how *Redress Review* can either fail to *Collate Data* or be transitioned to *Payment*. 
If it is failed back to *Collate Data* then *Collate Data*, *Adjustment Decision*, *AutoTransitionToRedressReview* and, if applicable, *Adjustment* will be reset.

```C#
            redressReview
                .AddTransition(payment)
                .AddTransition(collateData)
                .AddDependency(communication)
                .AddDependency(autoTransitionToRedressReview)
                .AddActionAsync(StateActionType.Entry, 
                        redressReview.CalculateFinalRedressAmountAsync);
                
            // ...
            // ...
            // ...
            
            result = await redressReview
            				.ExecuteAsync(StateStatus.Fail, collateData);                
```

![Alt text](/README-images/Dipstate-example-fail.png?raw=true "Fail a state")


#### Completing a Workflow

When a sub state completes, it will attempt to transition to another state. If it does not have a state to transition to it will complete itself. If the state is configured to complete its parent and all its siblings are complete then the parent will complete. This will continue up the hierarchy until finally the root workflow state is completed.

The following shows the *Payment* state configured to complete the workflow.

```C#
            result = await payment.ExecuteAsync(StateStatus.Complete);

            Assert.AreEqual(payment.Status, StateStatus.Complete);

            Assert.IsTrue(result.Equals(remediationWorkflowRoot));
            Assert.AreEqual(remediationWorkflowRoot.Status, StateStatus.Complete);
```

![Alt text](/README-images/Dipstate-example-complete-workflow.png?raw=true "Complete a workflow")
