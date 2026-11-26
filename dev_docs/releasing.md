# Releasing the package Mavalonia

The package `Mavalonia`, packed from `Mavalonia.Markdown.Controls` with `Mavalonia.Markdown` inside it, is published to
nuget.org by `.github/workflows/publish.yml` when a tag such as `v0.8.0` is pushed. Its version is the tag's, by MinVer;
untagged builds are versioned as prereleases of the next one.

Publishing is Magnus's to decide: an agent pushes a version tag only on his word.

1. In `CHANGELOG.md`, move what is to be released from `## [Unreleased]` into a section `## [<version>] - <date>`. The
   publish fails without that section, whose text becomes the GitHub release's notes.
2. Commit, push, and see CI pass.
3. `git tag v<version>`, then `git push origin v<version>`.

The workflow then builds and runs the specifications, packs the package, gets a short-lived API key from nuget.org,
pushes the package, and makes a GitHub release of it.

## Trusted Publishing

No API key is stored anywhere. GitHub gives the workflow a signed token that says which repository and workflow file it
is. nuget.org checks that against a policy of Magnus's account, and gives the workflow an API key that is good for one
hour. The policy, under Trusted Publishing on nuget.org:

| Field | Value |
| --- | --- |
| Package owner | MagnusLidbom |
| Repository owner | mlidbom |
| Repository | Mavalonia |
| Workflow file | publish.yml |
| Environment | (none) |

Renaming the workflow file or the repository stops publishing until the policy is changed to match.
