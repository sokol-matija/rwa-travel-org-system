export default {
  extends: ["@commitlint/config-conventional"],
  rules: {
    // Type enum - what types of commits are allowed
    "type-enum": [
      2,
      "always",
      [
        "feat", // New feature
        "fix", // Bug fix
        "docs", // Documentation only changes
        "style", // Code style changes (formatting, missing semi colons, etc)
        "refactor", // Code refactoring (neither fixes a bug nor adds a feature)
        "perf", // Performance improvements
        "test", // Adding or updating tests
        "chore", // Maintenance tasks (dependencies, configs, etc)
        "build", // Build system changes
        "ci", // CI configuration changes
        "revert", // Reverting previous commits
      ],
    ],
    // Subject (description) must not be empty
    "subject-empty": [2, "never"],
    // Subject must not end with a period
    "subject-full-stop": [2, "never", "."],
    // Type must not be empty
    "type-empty": [2, "never"],
    // Type must be lowercase
    "type-case": [2, "always", "lower-case"],
  },
};
