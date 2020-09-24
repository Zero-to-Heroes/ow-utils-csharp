Basic implementation of a screenshot plugin.

It returns both the path to the file (to share on social media) and a base64 representation of the image, to display in the app (in my case, as a preview).

In this implementation, the image name is fixed, meaning that screenshots will override each other (as I only need the image to share it on social media). This can of course be changed if you need it :)

For an implementation of this plugin inside the app (in Angular), please have a look at:
* the https://github.com/Zero-to-Heroes/firestone/blob/master/core/src/js/services/plugins/ow-utils.service.ts service for the plugin declaration
* the https://github.com/Zero-to-Heroes/firestone/tree/master/core/src/js/components/sharing package for using it. The `onSocialShare` handler is defined here: https://github.com/Zero-to-Heroes/firestone/blob/master/core/src/js/components/battlegrounds/post-match/bgs-post-match-stats.component.ts#L243 (which actually calls the plugin)
