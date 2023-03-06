# M365AIMate

This project is part of the Hack Together: Microsoft Graph and .NET 🦒 running from March 1st to March 15th 2023.
[![Hack Together: Microsoft Graph and .NET](https://img.shields.io/badge/Microsoft%20-Hack--Together-orange?style=for-the-badge&logo=microsoft)](https://github.com/microsoft/hack-together)

Our project members are: [@appieschot](https://github.com/appieschot) [@RickVanRousselt](https://github.com/RickVanRousselt) [@stephanbisser](https://github.com/stephanbisser) and [@thomyg](https://github.com/thomyg)

M365AIMate is a tool to help you to get quickly up to speed with a demo environment for Microsoft 365. The goal is to have a tool that creates a defined environment for you to showcase the capabilities of Microsoft 365 and also adds initial content to the environment.

The Microsoft 365 objects are created using the Microsoft Graph API and the content is created by using OpenApi.

To get the most out of our efforts, the solution is split up into different projects. The main project is the **M365AIMate.Core** project. This project contains the code to create the environment and the content. The **M365AIMate.Core** project is a class library that can be used in other projects. The **M365AIMate.Console** project is a console application that uses the **M365AIMate.Core** project to create the environment and the content during testing and development. The **M365AIMate.TeamsTab** project is a Teams Tab UI that uses the **M365AIMate**.Core project to create the environment and the content inside a Teams Tab.

We also want to have test coverage for our core project. Therfore **M365AIMate.Core.Tests** project contains the tests for the **M365AIMate.Core** project.
