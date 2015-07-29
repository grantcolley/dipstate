# dipstate
A simple state machine for line-of-business applications.

![Alt text](/README-images/Dipstate-example-workflow.png?raw=true "Example workflow")

# Motivation
Dipstate provides a simple mechanism to maintain state for activities that are transitioned up and down a workflow. 

# Features
  * Support for async and synchronous execution
  * Action delegates run with context for entry, exit, reset and status changed events
  * Conditional transitioning between states
  * States can have sub-states
  * Support for auto states
  * Support for dependency states

