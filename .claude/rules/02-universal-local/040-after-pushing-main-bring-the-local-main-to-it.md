# After pushing to main, bring the local main to the same commit

Magnus develops in the checkout that has `main` checked out: `git worktree list` shows it, `C:\Dev\Mavalonia` on his
machine. Whenever you push to `origin/main`, fast-forward that local `main` to the same commit:
`git -C <that checkout> merge --ff-only origin/main`.

- **Why:** in his words (2026-11-26), "It is really confusing to me when origin/main is ahead of the only place where I
  develop". And the tooling won't remove a worktree, nor archive its session, while its branch isn't in the local
  `main`.
- **When the fast-forward fails,** such as on his uncommitted changes there, or commits of his own on `main`, leave his
  checkout as it is and tell him.
