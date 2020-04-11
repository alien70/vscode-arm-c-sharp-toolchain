In the previous [post](https://maurizioattanasi.blogspot.com/2020/03/set-up-c-arm-development-toolchain-with.html), I described the steps followed to set up a remote development toolchain for c++ programming on an ARM-based microcontroller (in my case, a [Friendly ELEC NanoPi M1 Plus](https://www.friendlyarm.com/index.php?route=product/product&product_id=176), but the same steps should be valid on the more popular [RaspberryPi](https://www.raspberrypi.org/) ).  

  

[![](https://1.bp.blogspot.com/-E0NMIEmwDrc/XoiyUqZUYGI/AAAAAAAALlM/qM52DZTl5JM7H7BJb-yJ5oXF0q3haMRCACLcBGAsYHQ/s320/M1plus_02-900x630.jpg)](https://1.bp.blogspot.com/-E0NMIEmwDrc/XoiyUqZUYGI/AAAAAAAALlM/qM52DZTl5JM7H7BJb-yJ5oXF0q3haMRCACLcBGAsYHQ/s1600/M1plus_02-900x630.jpg)

  

In this note, I'll try to do something similar but using the latest long time support version of dotnet core, 3.1 LTS (ver. 3.1.201 at the time of writing this post).

### Pi Setup

As for the C++ setup, we will use ssh to connect our debugger to the Pi. To debug we'll need root privileges, and to obtain this result we need to enable ssh connection using root.

To get this result we need to edit the sshd_config configuration file by running:

sudo nano /etc/ssh/sshd_config  

check if the line

PermitRootLogin yes  

exists or, in the case, add it and then reboot the Pi

sudo reboot  

#### .NET Core setup

First thing first we need to install .net core SDK on the Pi. Following the instruction available on the [sdk's download page](https://dotnet.microsoft.com/download/dotnet-core/thank-you/sdk-3.1.201-linux-arm32-binaries) on our ssh command line we will run the following commands:

  

~$ sudo apt-get install curl libunwind8 gettext
        
~$ wget https://download.visualstudio.microsoft.com/download/pr/ccbcbf70-9911-40b1-a8cf-e018a13e720e/03c0621c6510f9c6f4cca6951f2cc1a4/dotnet-sdk-3.1.201-linux-arm.tar.gz                 
        

  

The next step is to extract the packages we downloaded, and make the dotnet commands available at the terminal

  

~$ mkdir -p $HOME/dotnet && tar zxf dotnet-sdk-3.1.201-linux-arm.tar.gz -C $HOME/dotnet
~$ export DOTNET_ROOT=$HOME/dotnet
~$ export PATH=$PATH:$HOME/dotnet
        

  
In order to verify the installation, in our ssh terminal, run the dotnet --info command. If everything is ok something like this should appear.  

[![](https://1.bp.blogspot.com/-Feip_l1_Q2A/XomKUHVFeBI/AAAAAAAALlY/GLvRaTW8T9gw-Kovi-Nw-WNi0XY0a8vNQCLcBGAsYHQ/s320/dotnet-info.png)](https://1.bp.blogspot.com/-Feip_l1_Q2A/XomKUHVFeBI/AAAAAAAALlY/GLvRaTW8T9gw-Kovi-Nw-WNi0XY0a8vNQCLcBGAsYHQ/s1600/dotnet-info.png)

As stated in the .NET Core download page

  

The above commands will only make the .NET SDK commands available for the terminal session in which it was run.  
You can edit your shell profile to permanently add the commands. There are a number of different shells available for Linux and each has a different profile. For example:  

*   Bash Shell: ~/.bash_profile, ~/.bashrc
*   Korn Shell: ~/.kshrc or .profile
*   Z Shell: ~/.zshrc or .zprofile

Edit the appropriate source file for you shell and add :$HOME/dotnet to the end of the existing PATH statement. If no PATH statement is included, add a new line with export PATH=$PATH:$HOME/dotnet.  
Also, add export DOTNET_ROOT=$HOME/dotnet to the end of the file.

#### VS Remote Debugger Setup

To install the vscode remote debugger on the NanoPi, on the ssh terminal rin the command:

  

~$ curl -sSL https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l ~/vsdbg

### Visual Studio Code remote debugging test

Now it's time to test the whole toolchain.

  

On our development machine (in my case an old MacBook Pro 15), we need the dotnet SDK installed, and, of course our favorite code editor [Visual Studio Code](https://code.visualstudio.com/).

  

To achieve the goal, we will use a simple console application named toolchain-test

  

In order to verify the machine on which we are running our simple test program we'll change the code as follows:  
  

    using System;

    namespace toolchain_test
    {
        class Program
        {
            static void Main(string\[\] args)
            {
                var logName = Environment.GetEnvironmentVariable("LOGNAME");
                Console.WriteLine($"Hello World! from {logName}");
            }
        }
    }                
            

  

As we did for the [C++ toolchain](https://maurizioattanasi.blogspot.com/2020/03/set-up-c-arm-development-toolchain-with.html) we will make use of [rsync](https://linux.die.net/man/1/rsync), a copying tool available on macOS, Linux, and now also on Windows 10 via the Windows Subsystem for Linux or [WSL2](https://docs.microsoft.com/it-it/windows/wsl/about) so our task.json and launch.json files will be:

  

        {
            "version": "2.0.0",
            "tasks": \[
                {
                    "label": "build",
                    "command": "dotnet",
                    "type": "process",
                    "args": \[
                        "build",
                        "${workspaceFolder}/toolchain-test/toolchain-test.csproj",
                        "/property:GenerateFullPaths=true",
                        "/consoleloggerparameters:NoSummary"
                    \],
                    "problemMatcher": "$msCompile"
                },  
                {
                    "label": "publish",
                    "command": "dotnet",
                    "dependsOn": "build",
                    "type": "process",
                    "args": \[
                        "publish",
                        "-r",
                        "linux-arm",
                        "${workspaceFolder}/toolchain-test/toolchain-test.csproj",
                        "/property:GenerateFullPaths=true",
                        "/consoleloggerparameters:NoSummary"
                    \],
                    "problemMatcher": "$msCompile"
                },
                {
                    "label": "rsync",
                    "type": "shell",
                    "dependsOn": "publish",
                    "osx": {
                        "command": "rsync -r -a -v -e ssh --delete toolchain-test/bin/Debug/netcoreapp3.1/linux-arm/ pi@192.168.1.177:/home/pi/projects/toolchain-test/"
                    }
                },
                {
                    "label": "watch",
                    "command": "dotnet",
                    "type": "process",
                    "args": \[
                        "watch",
                        "run",
                        "${workspaceFolder}/toolchain-test/toolchain-test.csproj",
                        "/property:GenerateFullPaths=true",
                        "/consoleloggerparameters:NoSummary"
                    \],
                    "problemMatcher": "$msCompile"
                }
            \]
        }        
    

  

        {
            // Use IntelliSense to find out which attributes exist for C# debugging
            // Use hover for the description of the existing attributes
            // For further information visit https://github.com/OmniSharp/omnisharp-vscode/blob/master/debugger-launchjson.md
            "version": "0.2.0",
            "configurations": \[
                {
                    "name": ".NET Core Launch (console)",
                    "type": "coreclr",
                    "request": "launch",
                    "preLaunchTask": "build",
                    "program": "${workspaceFolder}/toolchain-test/bin/Debug/netcoreapp3.1/toolchain-test.dll",
                    "args": \[\],
                    "cwd": "${workspaceFolder}",
                    "console": "internalConsole",
                    "stopAtEntry": false
                },
                {
                    "name": ".NET Core Remote Launch - Standalone Application (console)",
                    "type": "coreclr",
                    "request": "launch",
                    "program": "toolchain-test",
                    "args": \[\],
                    "cwd": "~/projects/toolchain-test",
                    "stopAtEntry": false,
                    "console": "internalConsole",
                    "pipeTransport": {
                        "pipeCwd": "${workspaceRoot}",
                        "pipeProgram": "/usr/bin/ssh",
                        "pipeArgs": \[
                            "-T",
                            "pi@192.168.1.177"
                        \],
                        "debuggerPath": "~/vsdbg/vsdbg"
                    },
                    "preLaunchTask": "rsync",
                },        
                {
                    "name": ".NET Core Attach",
                    "type": "coreclr",
                    "request": "attach",
                    "processId": "${command:pickProcess}"
                }
            \]
        }        
    

  

In the launch.json we have two configurations, a local and a remote one.  
  
Selecting the local one:  
  

[![](https://1.bp.blogspot.com/-ft_g5o6XLvE/XomsFft3w0I/AAAAAAAALlk/Y6CO5j3ne6oPpLCGvOdu1BDKm6MLV_QHwCLcBGAsYHQ/s320/local-debug.png)](https://1.bp.blogspot.com/-ft_g5o6XLvE/XomsFft3w0I/AAAAAAAALlk/Y6CO5j3ne6oPpLCGvOdu1BDKm6MLV_QHwCLcBGAsYHQ/s1600/local-debug.png)

  
and pressing F5, the program will stop at the breakpoint as we can see in the following picture:  
  

[![](https://1.bp.blogspot.com/-9Tfi-F_qUdo/Xomt76C0L6I/AAAAAAAALlw/v9syJBVO5CQpNp580GnAxc8KS6Lp4mPxACLcBGAsYHQ/s320/local-breakpopint.png)](https://1.bp.blogspot.com/-9Tfi-F_qUdo/Xomt76C0L6I/AAAAAAAALlw/v9syJBVO5CQpNp580GnAxc8KS6Lp4mPxACLcBGAsYHQ/s1600/local-breakpopint.png)

  
and the program output will be:  
  

[![](https://1.bp.blogspot.com/-_qc4IpCu06I/XomuKWxeEQI/AAAAAAAALl0/iLOgU0AoXUwfVVpTsQIB621WwujloYSXwCLcBGAsYHQ/s1600/local-console.png)](https://1.bp.blogspot.com/-_qc4IpCu06I/XomuKWxeEQI/AAAAAAAALl0/iLOgU0AoXUwfVVpTsQIB621WwujloYSXwCLcBGAsYHQ/s1600/local-console.png)

  
Selecting the remote debug configuration, we will have instead:  

[![](https://1.bp.blogspot.com/-eBNUdBsQdkE/XomugFplkwI/AAAAAAAALmA/xRt_lb_IwNMs_m-RY-h8Lk-YP6prWDkvQCLcBGAsYHQ/s320/remote-debug.png)](https://1.bp.blogspot.com/-eBNUdBsQdkE/XomugFplkwI/AAAAAAAALmA/xRt_lb_IwNMs_m-RY-h8Lk-YP6prWDkvQCLcBGAsYHQ/s1600/remote-debug.png)

[![](https://1.bp.blogspot.com/-g_jLCT_y2NY/XomudpfIhsI/AAAAAAAALl8/BCZuOSbOGVopIBRICsCVIgMZxI2_TY8jQCEwYBhgLKs4DAMBZVoA4veEi53RtOoVPwB2jiUcCHIADM0Bw-f0GDIRNZ_3ocF_KoJWW4gggVC9iUdxIHD0Aw6dY7D_pzje67A7H15XjdHRTKU3MDaQ4Qbd-rbl89acdSmf9hWCE4z_H6IkbfBPxoezSOh-Csoj3dkWUPO6I88qbAJ5lRW7TMIgJRSpjh2wfIquGpmlJo6JqjdhMuq1gJLZRb6Woyxj6_Obq0YCtT0mNLjIcNp3ddznJEIO6tinA54FBKztWmjlcMIlUi8dmUcesdepEHoq26aDmbN84HchQG88RnXVSCdnl0_nIndqxhNZvgC1cMmZGX_YMqXNeGMh8GjNsB5uDoD9SUn9Blc0wz_J_VP9maj_GcxEZAxcF4y9otFkBXSHX75422dAbiSZkrP16_jfK-GIP-lNaM1HZmYouiumibzvt87ntNaaTpBRQXIgkKyr2zOa6XYxpV9ik4hHrCueDPLp0mSmbMa-E64I39HCwjM2YNeNU9ofdGrOYIMy4vbutxPD0oQYAa4Btam4goOw--i0Vez-GgbhK-PlaAW_4zlkh7vuLrIQrqS3NQ6tZy8ljhzqkfhhHvrc9hfnKV3LK0HTFWAA8MyvcG5JIpx_XMO7hpvQF/s320/remote-breakpoint.png)](https://1.bp.blogspot.com/-g_jLCT_y2NY/XomudpfIhsI/AAAAAAAALl8/BCZuOSbOGVopIBRICsCVIgMZxI2_TY8jQCEwYBhgLKs4DAMBZVoA4veEi53RtOoVPwB2jiUcCHIADM0Bw-f0GDIRNZ_3ocF_KoJWW4gggVC9iUdxIHD0Aw6dY7D_pzje67A7H15XjdHRTKU3MDaQ4Qbd-rbl89acdSmf9hWCE4z_H6IkbfBPxoezSOh-Csoj3dkWUPO6I88qbAJ5lRW7TMIgJRSpjh2wfIquGpmlJo6JqjdhMuq1gJLZRb6Woyxj6_Obq0YCtT0mNLjIcNp3ddznJEIO6tinA54FBKztWmjlcMIlUi8dmUcesdepEHoq26aDmbN84HchQG88RnXVSCdnl0_nIndqxhNZvgC1cMmZGX_YMqXNeGMh8GjNsB5uDoD9SUn9Blc0wz_J_VP9maj_GcxEZAxcF4y9otFkBXSHX75422dAbiSZkrP16_jfK-GIP-lNaM1HZmYouiumibzvt87ntNaaTpBRQXIgkKyr2zOa6XYxpV9ik4hHrCueDPLp0mSmbMa-E64I39HCwjM2YNeNU9ofdGrOYIMy4vbutxPD0oQYAa4Btam4goOw--i0Vez-GgbhK-PlaAW_4zlkh7vuLrIQrqS3NQ6tZy8ljhzqkfhhHvrc9hfnKV3LK0HTFWAA8MyvcG5JIpx_XMO7hpvQF/s1600/remote-breakpoint.png)

[![](https://1.bp.blogspot.com/-d59rcv8qDpg/XomuqIJLvqI/AAAAAAAALmE/7dNE24iskVE7G5wsP7HhqgECl4Z5Xu1TwCLcBGAsYHQ/s1600/remote-console.png)](https://1.bp.blogspot.com/-d59rcv8qDpg/XomuqIJLvqI/AAAAAAAALmE/7dNE24iskVE7G5wsP7HhqgECl4Z5Xu1TwCLcBGAsYHQ/s1600/remote-console.png)
  
As we can see, in the first test, the logName variable assumes the development machine LOGNAME environment variable, while in the second it assumes the Pi's one.

As always Enjoy, and if we are still under COVID-19 attack, #staysafe, #StayAtHome.  

[![](https://1.bp.blogspot.com/-VPpx7wpWqMk/Xom1ZaYbINI/AAAAAAAALmU/srDW2xp9I_kHGJHP2J_Q2QE4-APN0vPZQCLcBGAsYHQ/s320/stay-at-home_2x.png)](https://1.bp.blogspot.com/-VPpx7wpWqMk/Xom1ZaYbINI/AAAAAAAALmU/srDW2xp9I_kHGJHP2J_Q2QE4-APN0vPZQCLcBGAsYHQ/s1600/stay-at-home_2x.png)