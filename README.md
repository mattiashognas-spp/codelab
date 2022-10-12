# CodeLab

## Information
```src\api``` project - this is where you add your implementation.

```src\tests``` project - this has the requirements implemented as a test.

## Scenario:
We need an endpoint that can return top combined values in insurances with depth restaints.

### Rules:
 - The Value property of the Insurance model is the property we like to combine for our results.
 - We need to be able to set the maximum amount of returned values. Utilize the maxCount parameter.
 - We need to be able to limit the depth of children calulated. Utilize the maxDepth parameter.

## Hints
Seed data can be found in the Seed method. If you want to change it, feel free to do so.

However, know that the "expectedValues" in the theory are based on initial seed data and could give you a hint toward the implementation.