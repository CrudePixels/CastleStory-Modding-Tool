# Creating a GitHub Release

## Option 1: Using GitHub Web Interface (Easiest)

1. Go to: https://github.com/CrudePixels/CastleStory-Modding-Tool/releases
2. Click **"Draft a new release"**
3. Fill in the details:
   - **Tag version**: `v1.7.0`
   - **Release title**: `v1.7.0 - Jason's Enhancements Mod`
   - **Description**: Copy the contents from `RELEASE_NOTES.md`
4. Click **"Publish release"**

## Option 2: Using PowerShell Script

1. Get a GitHub Personal Access Token:
   - Go to: https://github.com/settings/tokens
   - Click "Generate new token (classic)"
   - Select scope: `repo` (full control of private repositories)
   - Copy the token

2. Run the script:
   ```powershell
   $env:GITHUB_TOKEN = "your_token_here"
   .\scripts\create-release.ps1 -Version "v1.7.0"
   ```

## Option 3: Using GitHub CLI (if installed)

```bash
gh release create v1.7.0 --title "v1.7.0 - Jason's Enhancements Mod" --notes-file RELEASE_NOTES.md
```

## What's Already Done

✅ All code has been pushed to GitHub
✅ Git tag `v1.7.0` has been created and pushed
✅ Release notes have been created in `RELEASE_NOTES.md`

## Next Steps

Just create the release using one of the methods above. The tag is already on GitHub, so you can reference it when creating the release.
