# dipstate
Dipstate provides a simple mechanism to maintain state for an activity based workflow.

## Features
  * Support for async and synchronous execution
  * Action delegates execute with context for entry, exit, reset and status changed events
  * Conditional transitioning between states
  * States can have sub-states
  * Support for auto states
  * Support for dependency states

## State
### State Members
  * **Id** – the identifier of the state
  * **Name** - the name of the state
  * **Status** - the status of the state
  * **IsDirty** - indicates whether the status of the state has changed
  * **InitialiseWithParent** - indicates whether the state will be initialised when its parent is initialised
  * **CompletionRequired** - indicates whether completion of the state is required in order for its parent to complete
  * **Context** - the states context
  * **Type** - the type of state
  * **Parent** - the parent of the state
  * **Antecedent** - the preceding state in a workflow from which the state was transitioned from        
  * **Transition** - the state to transition to
  * **Transitions** - a list of states from which the state can transition to
  * **Dependencies** - a list of dependency states that must be completed before the state can be initialised.
  * **Dependants** - a list of states that are dependent on the state being completed before they can be initialised
  * **SubStates** - a list of sub states. Sub states can behave as mini workflows under their parent
  * **Log** - the state log
  * **Actions** - a list of action delegates that are executed at different stages in the lifecycle of the state

### State Types
  * **Root** is a state that represents a workflow. Its sub states are the states within the workflow. Initialising the root state is the entry point into the workflow and is automatically completed when the last sub state requiring completion has completed.
  * **Auto** is a state which will automatically complete itself after initialisation. Entry actions are executed during the initialisation which is a good place to perform some task or determine the state it needs to transition to at runtime..
  * **Standard** is a plain vanilla state.

*Creating different types of states:*
```C#
            var remediationWorkflowRoot 
                = new State(100, "Remediation Workflow", StateType.Root);
                
            // unless otherwise specified a standard state is created
            var collateData = new State(300, "Collate Data");
            
            var adjustmentDecision 
                = new State(400, "Adjustment Decision", StateType.Auto);
```

### State Delegates
**Action Delegates** are executed at different stages in the lifecycle of the state.
  * **OnEntry**
  * **OnStatusChanged**
  * **OnExit**
  * **Reset**

**Predicate Delegates** are executed prior to performing an execution against a state.
  * **CanInitialiseState**
  * **CanChangeStateStatus**
  * **CanCompleteState**
  * **CanResetState**

*Setting up async state delegates:*
```C#
            var letterSent = new State(210, "Letter Sent")
                .AddActionAsync(StateActionType.OnEntry, GenerateLetterAsync)
                .AddActionAsync(StateActionType.OnStatusChanged, SaveStatusAsync)
                .AddActionAsync(StateActionType.OnExit, NotifyDispatchAsync)
                .AddCanInitialisePredicateAsync(CanInitialiseLetterSentAsync)
                .AddCanChangeStatusPredicateAsync(CanChangeLetterSentStatusAsync)
                .AddCanCompletePredicateAsync(CanCompleteLetterSentAsync)
                .AddCanResetPredicateAsync(CanResetLetterSentAsync);
                
                
                
                
        public static async Task GenerateLetterAsync(State context)
        {
            // do entry actions here...
        }
        
        private static async Task SaveStatusAsync(State context)
        {
            // do status changed actions here...
        }

        private static async Task NotifyDispatchAsync(State context)
        {
            // do exit actions here...
        }
        
        public static async Task<bool> CanInitialiseLetterSentAsync(State context)
        {
            // determine wether the state can be initialised here...
        }

        public static async Task<bool> CanChangeLetterSentStatusAsync(State context)
        {
            // determine whether the status can be changed here...
        }

        public static async Task<bool> CanCompleteLetterSentAsync(State context)
        {
            // determine whether the state can be completed here...
        }

        public static async Task<bool> CanResetLetterSentAsync(State context)
        {
            // determine whether the state can be reset here...
        }
```

## How it works

Here is how it works by way of an example workflow. 

**Note:**
For a full listing of the code see test class [GitHubReadMeExampleTest.cs](https://github.com/grantcolley/dipstate/tree/master/DevelopmentInProgress.DipState.Test/GitHubReadMeExampleTest.cs) in [DevelopmentInProgress.DipState.Test](https://github.com/grantcolley/dipstate/tree/master/DevelopmentInProgress.DipState.Test) project. You can also find an example WPF implementation of the workflow at [Origin](https://github.com/grantcolley/origin).

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
                .AddDependant(redressReview)
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
                .AddDependant(redressReview)
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

#### Initialise a State
  * Initialisation will not succeed if a state has one or more **dependency states** that have not yet completed.
  * During initialisation the **OnEntry** actions are executed with context.
  * When the status changes to Initialised the **OnStatusChanged** actions are executed with context.
  * If the state type is **Auto** then, if a transition state is specified, the state will transition to it. An **OnEntry** action can be used to determine at runtime which state to transition to. Alternatively, if no transition state has been set, the state will complete itself.
  * If the state has sub states then those sub states where **InitialiseWithParent** is set to true will also be initialised.

> **_WARNING:_**
> Initialising a sub state directly, without first initialising its parent 
> will result in the parent’s status being updated to Initialised without 
> running its **CanInitialise** predicate or its **OnEntry** and **OnStatusChanged** actions. 
> Consider setting the sub state to initialise with parent and then initialise 
> the parent instead. This way all initialisation predicates and actions will 
> be run for both the parent and the child.

The following shows how the initialising the *Remediation Workflow Root* will also initialise *Collate Data*, *Communication* and its sub state *Letter Sent*.

```C#
            var result = await remediationWorkflowRoot
                                            .ExecuteAsync(StateExecutionType.Initialise);

            Assert.IsTrue(result.Equals(remediationWorkflowRoot));

            Assert.AreEqual(remediationWorkflowRoot.Status, StateStatus.Initialised);
            Assert.AreEqual(communication.Status, StateStatus.Initialised);
            Assert.AreEqual(letterSent.Status, StateStatus.Initialised);
```

![Alt text](/README-images/Dipstate-example-initialiseState.png?raw=true "Initialising a state")

#### Transition a State
  * A state can only transition to another state that is in its **Transition** list.
  * When adding a transition state to the **Transition** list the **IsDefaultTransition** flag can be optionally set. A state can only have one default transition state.
  * Transitioning from one state to another state will complete the state being transition from and initialise the state being transitioned to. See [Complete a State](#complete-a-state) for details of completing a state.
  * A state can transition explicitly or implicitly. If the state is **Completed** rather than explicitly transitioned to another state, the state will attempt to transition to the default state if one has been set.

The following shows *Letter Sent* explicitly transition to *Response*.

```C#
            result = await letterSent.ExecuteAsync(responseReceived);

            Assert.IsTrue(result.Equals(responseReceived));
            Assert.IsTrue(responseReceived.Antecedent.Equals(letterSent));
            Assert.AreEqual(responseReceived.Status, StateStatus.Initialised);
            Assert.AreEqual(letterSent.Status, StateStatus.Completed);
```

![Alt text](/README-images/Dipstate-example-transition.png?raw=true "Transition a state")

#### Complete a State
  * First, the **CanComplete** delegate is executed to determine whether the state can complete. If no delegate has been provided the state will complete.
  * **OnExit** actions are executed with context before completing the state.
  * The status is set to complete and **OnStatusChanged** actions are executed with context.
  * Any dependant states with **InitialiseDependantWhenComplete** set to true will be initialised
  * If a transition state has been specified then the state will transition. See [Transition a State](#transition-a-state) for more details of transitioning.
  * If no transition state has been specified the state will check if all its parents sub states requiring completion (**CompletionRequired** set to true) has completed and, if they have, it will complete the parent.

> **_TIP:_**
> It is better to transition at the parent level when all its sub states have completed. 
> When the last sub state with CompletionRequired set to true, has completed it will attempt 
> to complete its parent. If a transition is required it is better to 

> **_WARNING:_**
> When the same state can be both a dependant and a transition state for another state, do not set both 
> **InitialiseDependantWhenComplete** and **IsDefaultTransition** flags to true or it will get initialised 
> twice (as a dependent and a transition state), and its **OnEntry** actions will be executed twice.

The following shows how *ResponseReceived* completes itself and its parent, *Communication*, which is in turn configured to transition to Redress Review by default when it completes. Note that when adding *RedressReview* to *Communication* as a dependent, **InitialiseDependantWhenComplete has** not been explicitly set to true (it is false by default. Therefore when *Communication* has been completed *RedressReview* will only get initialised once as the state being transitioned to.

```C#
            communication
                .AddSubState(letterSent, true)
                .AddSubState(responseReceived)
                .AddDependant(redressReview)
                .AddTransition(redressReview, true);

            // ...
            // ...
            // ...
            
            result = await responseReceived.ExecuteAsync(StateExecutionType.Complete);

            Assert.IsTrue(result.Equals(redressReview));
            Assert.IsTrue(redressReview.Antecedent.Equals(communication));
            Assert.AreEqual(communication.Status, StateStatus.Completed);
            Assert.AreEqual(letterSent.Status, StateStatus.Completed);
            Assert.AreEqual(responseReceived.Status, StateStatus.Completed);
            Assert.AreEqual(redressReview.Status, StateStatus.Initialised);
```

![Alt text](/README-images/Dipstate-example-substate-close-parent.png?raw=true "Sub state completes its parent")

#### Auto States
Auto states will either automatically transition or complete itself after it has been initialised.
**OnEntry** actions are delegates that enable processing with state context to take place. In the case of an auto state an **OnEntry** action can be used to determine at runtime which state to transition to.

The following example shows the configuration of auto state *AdjustmentDecision* which enables it to transition to either the *Adjustment* or *AutoTransitionToRedressReview* (which just happens to be another auto state). The **ConditionalTransitionDecisionAsync** entry action will determine whether to transition to *adjustment* or *autoTransitionToRedressReview* at runtime.

```C#
            adjustmentDecision
                .AddTransition(adjustment)
                .AddTransition(autoTransitionToRedressReview)
                .AddActionAsync(StateActionType.OnEntry, ConditionalTransitionDecisionAsync);
                
        private static async Task ConditionalTransitionDecisionAsync(State context)
        {
             // determine whether to transition to adjustment or autoTransitionToRedressReview here...
        }
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
