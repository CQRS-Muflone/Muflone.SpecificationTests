# Muflone.SpecificationTests

Helper for creating specification tests with Muflone

## Install

`Install-Package Muflone.SpecificationTests`

## Usage

Look at [this repo](https://github.com/CQRS-Muflone/CQRS-ES_testing_workshop)

## Repository mocking

In the test's constructor implement your mock and replace the Repository property

```C#
public void MyTest()
{
  var repo = new Mock<InMemoryEventRepository> { CallBase = true };
  repo.Setup(x => x.GetById<Product>(It.IsAny<string>(), productId)).Returns(product);
  Repository = repo.Object;
}

```
