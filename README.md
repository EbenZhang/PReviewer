# Introduction

**PReviewer** is a desktop application designed to review github pull requests. The key feature is the ability to view differences in external diff tools such as [Beyond Compare](http://www.scootersoftware.com), [KDiff](http://kdiff3.sourceforge.net/), [WinMerge](http://winmerge.org) or whatever you like.

# Features

In this release **PReviewer** provides the ability  to

 * Display the title and description of the pull request.
 * List changed files in a pull request.
 * open the changed files in external difftool, for example beyond compare.
 * Show the differences inside the application itself.
 * Comment on individual files, and submit a general comment. However inline comment on specific commit hasn't been supported yet.
 * Compare commits in the pull request.

# Install

Navigate to [here](https://raw.github.com/EbenZhang/PReviewer/master/dist/setup.exe) to download the setup.exe and install it following the instructions.

# License

PReview is a free tool and its [source code](https://github.com/ebenzhang/previewer) is published under [Microsoft Public License (MS-PL)](http://opensource.org/licenses/ms-pl.html)

All rights reserved [Nicologies](http://www.nicologies.tk) @2015

# Donate

If you feel that the work I provide is worth it, you can donate by using the Donate button below. This is greatly appreciated.
You can use Paypal to donate even if you do not have a Paypal account.There is, of course, no obligation whatsoever. 
If you prefer, you can rather get me a bottle of coke next time I am in a location near you ;) 

```html
<form action="https://www.paypal.com/cgi-bin/webscr" method="post" target="_top">
<input type="hidden" name="cmd" value="_donations">
<input type="hidden" name="business" value="nicoleflopy@gmail.com">
<input type="hidden" name="lc" value="AUS">
<input type="hidden" name="item_name" value="Nicologies">
<input type="hidden" name="no_note" value="0">
<input type="hidden" name="currency_code" value="AUD">
<input type="hidden" name="bn" value="PP-DonationsBF:btn_donateCC_LG.gif:NonHostedGuest">
<input type="image" src="https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif" border="0" name="submit" alt="PayPal - The safer, easier way to pay online!">
<img alt="" border="0" src="https://www.paypalobjects.com/en_US/i/scr/pixel.gif" width="1" height="1">
</form>
```

# Feedback

If you have any suggestion, find any bug, or just want to contribute, please submit to the [issues page](https://github.com/EbenZhang/PReviewer/issues/new).

# Coming Features

Here are the features may be added in the future but with no guarantee. I would be appreciated if you can [help](https://github.com/EbenZhang/PReviewer).

 * Inline comment on specific commit
 * Integrated markdown editor, probably a WYSIWYG editor, but this is low priority .
 * Replace the diffviewer With the wpf version of ICsharpcode.editor.
