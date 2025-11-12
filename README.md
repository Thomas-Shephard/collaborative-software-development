# collaborative-software-development

## GitHub Pull Request Titles

This project adheres to the [Conventional Commits specification](https://www.conventionalcommits.org/en/v1.0.0/) for
**GitHub Pull Request titles**. This standard provides a set of rules for creating an explicit and readable history in
the main branch.

The GitHub Pull Request title should be structured as follows:

```
<type>: <description>
```

### Type

The type must be one of the following:

* **feat**: A new feature
* **fix**: A bug fix
* **docs**: Documentation only changes
* **style**: Changes that do not affect the meaning of the code (white-space, formatting, missing semicolons, etc.)
* **refactor**: A code change that neither fixes a bug nor adds a feature
* **perf**: A code change that improves performance
* **test**: Adding missing tests or correcting existing tests
* **chore**: Changes to the build process or auxiliary tools and libraries such as documentation generation
* **build**: Changes that affect the build system or external dependencies
* **ci**: Changes to our CI configuration files and scripts
* **revert**: Reverts a previous commit

### Description

A short, imperative tense description of the change in lowercase.

The description should complete the sentence "This Pull Request will <description>".

### Example:

```
feat: implement user login functionality
```
