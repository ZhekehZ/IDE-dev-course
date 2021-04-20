package com.jetbrains.rider.ideaInterop.fileTypes.spring

import com.intellij.lexer.Lexer
import com.intellij.openapi.project.Project
import com.intellij.psi.tree.IElementType
import com.jetbrains.rider.ideaInterop.fileTypes.RiderFileElementType
import com.jetbrains.rider.ideaInterop.fileTypes.RiderParserDefinitionBase
import com.intellij.psi.tree.IFileElementType
import com.jetbrains.rider.ideaInterop.fileTypes.RiderDummyLexer

class SpringParserDefinition : RiderParserDefinitionBase(SpringFileElementType, SpringFileType) {
    companion object {
        val SpringElementType = IElementType("RIDER_SPRING", SpringLanguage)
        val SpringFileElementType = RiderFileElementType("RIDER_SPRING_FILE", SpringLanguage, SpringElementType)
    }

    override fun createLexer(project: Project?): Lexer = RiderDummyLexer(SpringLanguage)
    override fun getFileNodeType(): IFileElementType = SpringFileElementType
}
