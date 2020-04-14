     

In the wake of the previous note, [Set Up .NET Core 3.1 ARM Development Toolchain with Visual Studio Code](https://maurizioattanasi.blogspot.com/2020/04/set-up-net-core-31-arm-development.html), this time the goal is to check whether [Microsoft's .NET Core IoT Libraries](https://github.com/dotnet/iot) works also with the [NanoPi M1 Plus](https://www.friendlyarm.com/index.php?route=product/product&product_id=176&search=nanopi+m1+plus&description=true&category_id=0&sub_category=true) even if this device is not in the list of supported devices.  
  
  

[![](https://1.bp.blogspot.com/-v3upCyek1Js/XpSt0Eig-0I/AAAAAAAAMEo/faR7FPJr8AcXd59YuGESgPKaEqsdCfGDwCKgBGAsYHg/s320/IMG_20200413_185526.jpg)](https://1.bp.blogspot.com/-v3upCyek1Js/XpSt0Eig-0I/AAAAAAAAMEo/faR7FPJr8AcXd59YuGESgPKaEqsdCfGDwCKgBGAsYHg/s1600/IMG_20200413_185526.jpg)

  

Following the instructions in the [led-blink](https://github.com/dotnet/iot/blob/master/samples/led-blink/README.md) sample, we will connect pin 17 (GPIOA17) to the cathode of a LED interposing a resistor, and pin 39 (GND) to the anode.

  

    class Program
            {
                static void Main(string\[\] args)
                {
                    var pin = 17;
                    var lightTimeInMilliseconds = 1000;
                    var dimTimeInMilliseconds = 200;
        
                    Console.WriteLine($"Let's blink an LED!");
                    using (GpioController controller = new GpioController())
                    {
                        controller.OpenPin(pin, PinMode.Output);
                        Console.WriteLine($"GPIO pin enabled for use: {pin}");
        
                        Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs eventArgs) =>
                        {
                            controller.Dispose();
                        };
        
                        while (true)
                        {
                            Console.WriteLine($"Light for {lightTimeInMilliseconds}ms");
                            controller.Write(pin, PinValue.High);
                            Thread.Sleep(lightTimeInMilliseconds);
                            Console.WriteLine($"Dim for {dimTimeInMilliseconds}ms");
                            controller.Write(pin, PinValue.Low);
                            Thread.Sleep(dimTimeInMilliseconds);
                        }
                    }
                }
                

  

The code above, copied and pasted from the sample, is self-explanatory, in fact

1.  we select pin 17 and configure it as an output
    
      
    
    controller.OpenPin(pin, PinMode.Output);
    
  
3.  turn the LED on for 1 second
    
      
    
                        controller.Write(pin, PinValue.High);
                                Thread.Sleep(lightTimeInMilliseconds);
                            
    
  
5.  then we turn it off for 200 ms
    
      
    
                        controller.Write(pin, PinValue.Low);
                                Thread.Sleep(dimTimeInMilliseconds);                    
                            
    
      
    
6.  repeat from point 1.
    

As we've done in the previous note, we will configure our tasks.json and launch.json files in order to allow remote debugging via ssh

  

Only one _caveat_ has to be considered this time in order to allow vsdg to change the mode of a pin.

In fact, this operation requires root permissions, and to get this result, one possible way is to prepend sudo before the vsdbg path in the _launch.json_ configuration.

  

            "debuggerPath": "sudo ~/vsdbg/vsdbg"
                

  

The following video shows a remote debug test of the program, and demonstrate that.

*   The Microsoft .NET Core IoT Libraries works as expected;
*   We are able to use the remote debugger stopping code execution at our breakpoints

  

  

The code of this note is available [here](https://github.com/alien70/vscode-arm-c-sharp-toolchain) on my Github repo.

  

As always  
  
Enjoy,  
  
and if we are still under COVID-19 attack, #staysafe, #StayAtHome.  
  

[![](https://1.bp.blogspot.com/-VPpx7wpWqMk/Xom1ZaYbINI/AAAAAAAALmU/srDW2xp9I_kHGJHP2J_Q2QE4-APN0vPZQCLcBGAsYHQ/s320/stay-at-home_2x.png)](https://1.bp.blogspot.com/-VPpx7wpWqMk/Xom1ZaYbINI/AAAAAAAALmU/srDW2xp9I_kHGJHP2J_Q2QE4-APN0vPZQCLcBGAsYHQ/s1600/stay-at-home_2x.png)