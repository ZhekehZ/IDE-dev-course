package com.jetbrains.rider.ideaInterop.fileTypes.spring

import com.jetbrains.rider.ideaInterop.fileTypes.RiderLanguageBase

abstract class SpringLanguageBase(name: String) : RiderLanguageBase(name, name) {
    override fun isCaseSensitive() = true
}

object SpringLanguage : SpringLanguageBase("Spring")
