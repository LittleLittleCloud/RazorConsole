# Git Hooks Quick Start

**⚡ Enable git hooks in 30 seconds!**

## One Command Setup

```bash
git config core.hooksPath .githooks
```

That's it! 🎉

## What You Get

✅ **pre-commit**: Format check (~10s)  
✅ **pre-push**: Test check (~30s)  

## Verify It Works

```bash
# Check configuration
git config core.hooksPath
# Should output: .githooks

# Make any change and commit to test
echo "# test" >> README.md
git add README.md
git commit -m "test"
# Hook runs automatically! 🎯
```

## Bypass When Needed

```bash
# Skip hooks in emergencies
git commit --no-verify
git push --no-verify
```

## More Info

- [Full documentation](.githooks/README.md)
- [Usage demos](.githooks/DEMO.md)
- [Lefthook alternative](.githooks/LEFTHOOK_SETUP.md)
- [Statistics & research](.githooks/STATISTICS.md)

---

**Why use hooks?** Get instant feedback (30s) instead of waiting for CI (5-10 min)! 🚀
