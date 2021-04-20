import com.jetbrains.rd.generator.gradle.RdgenParams
import org.gradle.api.tasks.testing.logging.TestExceptionFormat
import org.jetbrains.intellij.IntelliJPlugin
import org.jetbrains.intellij.tasks.PrepareSandboxTask
import org.jetbrains.kotlin.daemon.common.toHexString
import org.jetbrains.kotlin.gradle.tasks.KotlinCompile

buildscript {
    repositories {
        maven { setUrl("https://cache-redirector.jetbrains.com/www.myget.org/F/rd-snapshots/maven") }
        maven { setUrl("https://cache-redirector.jetbrains.com/dl.bintray.com/kotlin/kotlin-eap") }
        mavenCentral()
    }
    dependencies {
        classpath("com.jetbrains.rd:rd-gen:0.192.2")
        classpath("org.jetbrains.kotlin:kotlin-gradle-plugin:1.3.10")
    }
}

plugins {
    id("org.jetbrains.intellij") version "0.4.7"
}

apply {
    plugin("kotlin")
    plugin("com.jetbrains.rdgen")
}

repositories {
    mavenCentral()
    maven { setUrl("https://cache-redirector.jetbrains.com/dl.bintray.com/kotlin/kotlin-eap") }
}

java {
    sourceCompatibility = JavaVersion.VERSION_1_8
    targetCompatibility = JavaVersion.VERSION_1_8
}


val baseVersion = "2019.2"
val buildCounter = ext.properties["build.number"] ?: "9999"
version = "$baseVersion.$buildCounter"

intellij {
    type = "RD"

    // Download a version of Rider to compile and run with. Either set `version` to
    // 'LATEST-TRUNK-SNAPSHOT' or 'LATEST-EAP-SNAPSHOT' or a known version.
    // This will download from www.jetbrains.com/intellij-repository/snapshots or
    // www.jetbrains.com/intellij-repository/releases, respectively.
    // Note that there's no guarantee that these are kept up to date
    // version = 'LATEST-TRUNK-SNAPSHOT'
    // If the build isn't available in intellij-repository, use an installed version via `localPath`
    // localPath = '/Users/matt/Library/Application Support/JetBrains/Toolbox/apps/Rider/ch-1/171.4089.265/Rider EAP.app/Contents'
    // localPath = "C:\\Users\\Ivan.Shakhov\\AppData\\Local\\JetBrains\\Toolbox\\apps\\Rider\\ch-0\\171.4456.459"
    // localPath = "C:\\Users\\ivan.pashchenko\\AppData\\Local\\JetBrains\\Toolbox\\apps\\Rider\\ch-0\\dev"
    // localPath = "C:\\Program Files\\JetBrains\\JetBrains Rider 192.2370.680"

    val dir = file("build/rider")
    if (dir.exists()) {
        logger.lifecycle("*** Using Rider SDK from local path " + dir.absolutePath)
        localPath = dir.absolutePath
    } else {
        logger.lifecycle("*** Using Rider SDK from intellij-snapshots repository")
        version = "$baseVersion"
    }

    instrumentCode = false
    downloadSources = false
    updateSinceUntilBuild = false

    // Workaround for https://youtrack.jetbrains.com/issue/IDEA-179607
    setPlugins("rider-plugins-appender")
}

val repoRoot = projectDir.parentFile!!
val resharperPluginPath = File(repoRoot, "Spring")
val buildConfiguration = ext.properties["BuildConfiguration"] ?: "Debug"

val libFiles = listOf<String>()

val pluginFiles = listOf(
    "Spring/bin/$buildConfiguration/net461/JetBrains.ReSharper.Plugins.Spring")

val nugetPackagesPath by lazy {
    val sdkPath = intellij.ideaDependency.classes

    println("SDK path: $sdkPath")
    val path = File(sdkPath, "lib/ReSharperHostSdk")

    println("NuGet packages: $path")
    if (!path.isDirectory) error("$path does not exist or not a directory")

    return@lazy path
}

val riderSdkPackageVersion by lazy {
    val sdkPackageName = "JetBrains.Rider.SDK"

    val regex = Regex("${Regex.escape(sdkPackageName)}\\.([\\d\\.]+.*)\\.nupkg")
    val version = nugetPackagesPath
        .listFiles()
        .mapNotNull { regex.matchEntire(it.name)?.groupValues?.drop(1)?.first() }
        .singleOrNull() ?: error("$sdkPackageName package is not found in $nugetPackagesPath (or multiple matches)")
    println("$sdkPackageName version is $version")

    return@lazy version
}

val nugetConfigPath = File(repoRoot, "NuGet.Config")
val riderSdkVersionPropsPath = File(resharperPluginPath, "RiderSdkPackageVersion.props")

val riderSpringTargetsGroup = "rider-spring"

fun File.writeTextIfChanged(content: String) {
    val bytes = content.toByteArray()

    if (!exists() || readBytes().toHexString() != bytes.toHexString()) {
        println("Writing $path")
        writeBytes(bytes)
    }
}

configure<RdgenParams> {
    val csOutput = File(repoRoot, "Spring/src/Spring/src/Protocol")
    val ktOutput = File(repoRoot, "rider-spring/src/main/java/com/jetbrains/rider/plugins/spring/protocol")

    verbose = true
    hashFolder = "build/rdgen"
    logger.info("Configuring rdgen params")
    classpath({
        logger.info("Calculating classpath for rdgen, intellij.ideaDependency is ${intellij.ideaDependency}")
        val sdkPath = intellij.ideaDependency.classes
        val rdLibDirectory = File(sdkPath, "lib/rd").canonicalFile

        "$rdLibDirectory/rider-model.jar"
    })
    sources(File(repoRoot, "rider-spring/protocol/src/kotlin/model"))
    packages = "model"

    generator {
        language = "kotlin"
        transform = "asis"
        root = "com.jetbrains.rider.model.nova.ide.IdeRoot"
        namespace = "com.jetbrains.rider.model"
        directory = "$ktOutput"
    }

    generator {
        language = "csharp"
        transform = "reversed"
        root = "com.jetbrains.rider.model.nova.ide.IdeRoot"
        namespace = "JetBrains.Rider.Model"
        directory = "$csOutput"
    }
}

tasks {
    withType<PrepareSandboxTask> {
        var files = libFiles + pluginFiles.map { "$it.dll" } + pluginFiles.map { "$it.pdb" }
        files = files.map { "$resharperPluginPath/src/$it" }

        files.forEach {
            from(it, { into("${intellij.pluginName}/dotnet") })
        }

        doLast {
            files.forEach {
                val file = file(it)
                if (!file.exists()) throw RuntimeException("File $file does not exist")
                logger.warn("$name: ${file.name} -> $destinationDir/${intellij.pluginName}/dotnet")
            }
        }
    }

    withType<KotlinCompile> {
        kotlinOptions.jvmTarget = "1.8"
        dependsOn("rdgen")
    }

    withType<Test> {
        useTestNG()
        testLogging {
            showStandardStreams = true
            exceptionFormat = TestExceptionFormat.FULL
        }
        val rerunSuccessfulTests = false
        outputs.upToDateWhen { !rerunSuccessfulTests }
        ignoreFailures = true
    }

    create("writeRiderSdkVersionProps") {
        group = riderSpringTargetsGroup
        doLast {
            riderSdkVersionPropsPath.writeTextIfChanged("""<Project>
  <PropertyGroup>
    <RiderSDKVersion>[$riderSdkPackageVersion]</RiderSDKVersion>
  </PropertyGroup>
</Project>
""")
        }

        getByName("buildSearchableOptions") {
            enabled = buildConfiguration == "Release"
        }
    }

    create("writeNuGetConfig") {
        group = riderSpringTargetsGroup
        doLast {
            nugetConfigPath.writeTextIfChanged("""<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="resharper-sdk" value="$nugetPackagesPath" />
  </packageSources>
</configuration>
""")
        }
    }

    getByName("assemble") {
        doLast {
            logger.lifecycle("Plugin version: $version")
            logger.lifecycle("##teamcity[buildNumber '$version']")
        }
    }

    create("prepare") {
        group = riderSpringTargetsGroup
        dependsOn("rdgen", "writeNuGetConfig", "writeRiderSdkVersionProps")
        doLast {
            exec {
                executable = "dotnet"
                args = listOf("restore", "$resharperPluginPath/Spring.sln")
            }
        }
    }

    create("buildReSharperPlugin") {
        group = riderSpringTargetsGroup
        dependsOn("prepare")
        doLast {
            exec {
                executable = "msbuild"
                args = listOf("$resharperPluginPath/Spring.sln")
            }
        }
    }

    task<Wrapper>("wrapper") {
        gradleVersion = "4.10"
        distributionType = Wrapper.DistributionType.ALL
        distributionUrl = "https://cache-redirector.jetbrains.com/services.gradle.org/distributions/gradle-$gradleVersion-all.zip"
    }
}

defaultTasks("prepare")

// workaround for https://youtrack.jetbrains.com/issue/RIDER-18697
dependencies {
    testCompile("xalan", "xalan", "2.7.2")
}