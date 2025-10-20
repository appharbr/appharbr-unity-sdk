# AppHarbr SDK for Unity

Ad quality solution for Unity publishers - blocks inappropriate ads and provides enhanced user experience.

## Installation

### Method 1: Via Git URL (Available Now - Recommended)

1. Open Unity Editor (2022.3 or later)
2. Go to **Window → Package Manager**
3. Click the **+** button in the top-left corner
4. Select **Add package from git URL**
5. Enter:
```
   https://github.com/appharbr/appharbr-unity-sdk.git?path=/AppHarbrSDK
```
6. Click **Add**

#### Install Specific Version
To install a specific version, add `#` followed by the version tag:
```
https://github.com/appharbr/appharbr-unity-sdk.git?path=/AppHarbrSDK#2.0.0
```

### Method 2: Via OpenUPM (Coming Soon)

Once approved by OpenUPM, you'll be able to install via scoped registry:

1. Add OpenUPM registry to your `Packages/manifest.json`:
```json
{
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": ["com.appharbr"]
    }
  ],
  "dependencies": {
    "com.appharbr.sdk": "2.0.0"
  }
}
```

2. Unity will automatically install the package

## Migrating from Legacy SDK

If you previously installed AppHarbr SDK by importing a `.unitypackage` file:

1. Install the new version using Method 1 above
2. The SDK will automatically detect your old installation in `Assets/AppHarbrSDK`
3. A dialog will appear asking if you want to remove the old version
4. Click **"Yes, Remove Old Version"**

**What happens during migration:**
- ✅ Old `Assets/AppHarbrSDK` folder is removed
- ✅ All meta files are cleaned up
- ✅ Your configuration is preserved
- ✅ SDK is now managed via Package Manager

The SDK will now be located in `Packages/com.appharbr.sdk` instead of `Assets/AppHarbrSDK`.

## Updating the SDK

### If installed via Git URL:
1. Open **Window → Package Manager**
2. Find "AppHarbr SDK"
3. Click **Update** button

Or remove and re-add with the latest Git URL.

### If using version-specific tag:
Update the version number in your Git URL to the desired version.

## Requirements

- **Unity:** 2022.3 or later
- **Platforms:** Android, iOS
- **Ad Networks:** Compatible with all major mediation platforms

## Features

- Ad quality monitoring and blocking
- Inappropriate content filtering
- Enhanced user experience
- Automatic legacy SDK migration
- Full Unity Package Manager support

## Documentation

- [Changelog](https://github.com/appharbr/appharbr-unity-sdk/releases)
- [Latest Release](https://github.com/appharbr/appharbr-unity-sdk/releases/latest)

## Support

For technical support or questions:
- **Email:** ido.ozdova@appharbr.com
- **Website:** https://appharbr.com
- **Issues:** https://github.com/appharbr/appharbr-unity-sdk/issues

## License

Copyright © 2025 AppHarbr. All rights reserved.
