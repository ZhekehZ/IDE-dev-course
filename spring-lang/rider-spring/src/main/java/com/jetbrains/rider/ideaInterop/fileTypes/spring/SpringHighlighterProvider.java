package com.jetbrains.rider.ideaInterop.fileTypes.spring;

import com.intellij.openapi.fileTypes.FileType;
import com.intellij.openapi.fileTypes.SyntaxHighlighter;
import com.intellij.openapi.fileTypes.SyntaxHighlighterFactory;
import com.intellij.openapi.fileTypes.SyntaxHighlighterProvider;
import com.intellij.openapi.project.Project;
import com.intellij.openapi.vfs.VirtualFile;
import org.jetbrains.annotations.NotNull;
import org.jetbrains.annotations.Nullable;

public class SpringHighlighterProvider extends SyntaxHighlighterFactory implements SyntaxHighlighterProvider {

    @Override
    public @NotNull SyntaxHighlighter getSyntaxHighlighter(@Nullable Project project, @Nullable VirtualFile virtualFile) {
        return new SpringSyntaxHighlighter();
    }

    @Override
    public @Nullable SyntaxHighlighter create(@NotNull FileType fileType, @Nullable Project project, @Nullable VirtualFile virtualFile) {
        return fileType instanceof SpringFileType ? new SpringSyntaxHighlighter() : null;
    }
}

