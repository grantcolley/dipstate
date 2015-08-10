# dipstate
Dipstate provides a simple mechanism to maintain state for an activity based workflow.

### Features
  * Support for async and synchronous execution
  * Action delegates run with context for entry, exit, reset and status changed events
  * Conditional transitioning between states
  * States can have sub-states
  * Support for auto states
  * Support for dependency states

## How it works

Here is how it works by way of an example workflow. 

**Note:**
 * for a full listing of the code see test class **GitHubReadMeExampleTest.cs** in the test project **DevelopmentInProgress.DipState.Test**.
 * you can see the example running in a WPF application at **DevelopmentInProgress.Origin**

The example workflow follows the activities of a customer remediation process. The process starts by sending out a letter to a customer informing them a redress is due on their account a response is required. While waiting for a response from the customer data pertaining to the redress is gathered and the amount to be redressed is calculated. If necessary an adjustment is made to the calculated amount. After the redress amount has been calculated and the response is received from the customer the case is sent for final review. If the review fails the case is sent back to be re-calculated. If the review passes then payment is made to the customer.

![Alt text](/README-images/Dipstate-example-workflow.png?raw=true "Example workflow")

![Alt text](/README-images/WPF-Example.PNG?raw=true "WPF implementation of the Dipstate example workflow in DevelopmentInProgress.Origin")

#### Setting up the workflow
```C#
            // Create the remediation workflow states

            var remediationWorkflowRoot = new State(100, "Remediation Workflow",
                type: StateType.Root);

            var communication = new State(200, "Communication",
                                                initialiseWithParent: true);

            var letterSent = new State(210, "Letter Sent", initialiseWithParent: true)
                .AddActionAsync(StateActionType.Entry, GenerateLetterAsync)
                .AddActionAsync(StateActionType.Status, SaveStatusAsync)
                .AddActionAsync(StateActionType.Exit, NotifyDispatchAsync)
                .AddCanCompletePredicateAsync(LetterCheckedAsync);

            var response = new State(220, "Response", canCompleteParent: true)
                .AddActionAsync(StateActionType.Status, SaveStatusAsync);

            var collateData = new State(300, "Collate Data", true)
                .AddCanCompletePredicateAsync(ValidateDataAsync);

            var adjustmentDecision = new State(400, "Adjustment Decision",
                type: StateType.Auto);

            var adjustment = new State(500, "Adjustment");

            var autoTransitionToRedressReview
                = new State(600, "Auto Transition To Redress Review",
                                    type: StateType.Auto);

            var redressReview = new State(700, "Redress Review");

            var payment = new State(800, "Payment", canCompleteParent: true);


            // Assemble the remediation workflow

            redressReview
                .AddTransition(payment)
                .AddTransition(collateData)
                .AddDependency(communication)
                .AddDependency(autoTransitionToRedressReview)
		.AddActionAsync(StateActionType.Entry, redressReview.CalculateFinalRedressAmountAsync);

            autoTransitionToRedressReview
                .AddDependant(redressReview, true)
                .AddTransition(redressReview);

            adjustment.AddTransition(autoTransitionToRedressReview);

            adjustmentDecision
                .AddTransition(adjustment)
                .AddTransition(autoTransitionToRedressReview)
                .AddActionAsync(StateActionType.Entry, AdjustmentRequiredCheckAsync);

            collateData
                .AddTransition(adjustmentDecision);

            letterSent.AddTransition(response);

            communication
                .AddDependant(redressReview, true)
                .AddSubState(letterSent)
                .AddSubState(response)
                .AddTransition(redressReview);

            remediationWorkflowRoot
                .AddSubState(communication)
                .AddSubState(collateData)
                .AddSubState(adjustmentDecision)
                .AddSubState(adjustment)
                .AddSubState(autoTransitionToRedressReview)
                .AddSubState(redressReview)
                .AddSubState(payment);
```
##### State Properties
  * **InitialiseWithParent** - applies to sub states and indicates it will be initialised when its parent is initialised.
  * **CanCompleteParent** - applies to sub states and indicates the state will attempt to complete its parent state after it has completed.
  * **Type**
    * **Root** is reserved for the main root state in a workflow. There can be only one root within a workflow.
    * **Auto** states will automatically complete itself and attempt to transition after it has been initialised.
    * **Standard** is a plain vanilla state.

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


#### Initialising a State
  * A state cannot initialise if it has one or more **dependency states** that have not yet completed.
  * **Entry actions** are executed with context. 
  * **StatusChanged actions** are executed with context. 
  * If the type is **StateType.Auto** the state will automatically transition. An entry action can determine at runtime which state to transition to. Alternatively, if no transition state has been set, the state will complete itself.  
  * If the state has sub states then those sub states where **InitialiseWithParent** is true will also be initialised. 

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


#### Transition a State
  * Transitioning to another state completes the state being transition from.
  * If the state has only one transition state then it will transition to that state when it is set to complete.
  * A delegate is executed to determine whether the state can complete. If no delegate has been provided it will return true.
  * **Exit actions** are executed with context. 
  * **Status actions** are executed with context after the status has changed. 
  * Any dependant states with **InitialiseDependantWhenComplete** set to true will be initialised.
  * The transition state is initialised

The following shows *Letter Sent* transition to *Response*.

```C#
            result = await letterSent.ExecuteAsync(response);
            
            Assert.IsTrue(result.Name.Equals("Response"));
            Assert.AreEqual(letterSent.Status, StateStatus.Complete);
            Assert.AreEqual(response.Status, StateStatus.Initialise);
            Assert.IsTrue(response.Antecedent.Equals(letterSent));
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
                .AddActionAsync(StateActionType.Entry, AdjustmentRequiredCheckAsync);
```

![Alt text](/README-images/Dipstate-example-autostate.png?raw=true "Auto state")


#### Sub State can complete its Parent
A sub state can be configured to complete its parent. Typically this will be the last sub state expected to complete under the parent. The last sub state must not be configured to transition to another state so that it can complete its parent. The parent can be configured to transition to another state.

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
When a state is failed back to another state, that state and all states in between are **reset**. The state being failed to must be in the transition list.

When a state is reset, its status is set to Uninitialised and Reset action delegates will be triggered.

The following shows how *Redress Review* can either fail to *Collate Data* or be transitioned to *Payment*. 
If it is failed back to *Collate Data* then *Collate Data*, *Adjustment Decision*, *AutoTransitionToRedressReview* and, if applicable, *Adjustment* will be reset.

```C#
            redressReview
                .AddTransition(payment)
                .AddTransition(collateData)
                .AddDependency(communication)
                .AddDependency(autoTransitionToRedressReview);
                
            // ...
            // ...
            // ...
            
            result = await redressReview.ExecuteAsync(StateStatus.Fail, collateData);                
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
