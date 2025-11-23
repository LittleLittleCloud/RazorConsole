# Preview Deployment Setup

This document describes how to set up preview deployments for pull requests.

## Overview

The repository uses Cloudflare Pages to automatically deploy preview versions of the website for every pull request. This allows reviewers to see the changes in action before merging.

## Setup Instructions

To enable preview deployments, you need to configure Cloudflare Pages and add the required secrets to your GitHub repository.

### 1. Create a Cloudflare Pages Project

1. Sign up for a [Cloudflare account](https://dash.cloudflare.com/sign-up) (free tier is sufficient)
2. Go to **Workers & Pages** in your Cloudflare dashboard
3. Create a new **Pages** project:
   - Click **Create application** → **Pages** → **Connect to Git**
   - Or use **Direct Upload** and configure the project name as `razorconsole`

### 2. Get Cloudflare API Credentials

1. Go to your Cloudflare dashboard
2. Navigate to **My Profile** → **API Tokens**
3. Create a new API token with **Cloudflare Pages Edit** permissions
4. Copy your **Account ID** from the Pages project settings

### 3. Add GitHub Secrets

Add the following secrets to your GitHub repository:

1. Go to your repository on GitHub
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret** and add:
   - `CLOUDFLARE_API_TOKEN`: Your Cloudflare API token
   - `CLOUDFLARE_ACCOUNT_ID`: Your Cloudflare account ID

### 4. Test the Workflow

1. Create a pull request with some changes to the website
2. The preview deployment workflow will automatically run
3. A comment will be posted on the PR with the preview URL
4. Each subsequent push to the PR will update the preview

## How It Works

- The `.github/workflows/preview-deploy.yml` workflow triggers on pull request events
- It builds the website using the same process as the production build
- The built site is deployed to Cloudflare Pages with a unique URL for the PR
- A bot comment is added/updated on the PR with the preview URL
- The preview is automatically updated when new commits are pushed

## Alternative: Without Cloudflare

If you prefer not to use Cloudflare Pages, you can:

1. Use GitHub Pages with separate branches (requires additional configuration)
2. Use Netlify, Vercel, or another similar service
3. Simply upload the built site as a GitHub Actions artifact for manual review

To disable preview deployments, delete the `.github/workflows/preview-deploy.yml` file.
