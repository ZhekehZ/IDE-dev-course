package com.jetbrains.rider.ideaInterop.fileTypes.spring;

import com.jetbrains.rider.ideaInterop.fileTypes.RiderDummySyntaxHighlighter;

public class SpringSyntaxHighlighter extends RiderDummySyntaxHighlighter {

    public SpringSyntaxHighlighter() {
        super(SpringLanguage.INSTANCE);
    }
}
