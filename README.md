# DvTime - Functional Clocks Mod
This mod makes the various clocks around the Valley work.

## Commands
The time shown by this mod can be configured by various console commands:

### `time.source`
If called without arguments it will show how time currently progresses  
Call with the id of a timesource to set it.  
Call with an invalid id to list all availaible sources. Default sources: `realtime`, `playtime`.  
Other mods may add additional sources for you to use.  
`realtime` and `playtime` differ in that the real time always progresses while play time only progresses while in game.

### `time.current`
Get/set the current time

### `time.scale`
Get/set a factor for how fast time progresses.  
Value `x=1` is as fast as real time.  
Values `x>1` a faster than real time.  
Values `0<x<1` are slower than real time.  
Value `x=0` is stopped time.
Values `x<0` are time running backwards.  

### `time.reset`
Reset the current time source to default values as if you had started a fresh save for it.

## Consuming the time from other mods
Add a reference to `DvTime.dll` to your project.  

If DvTime may be a hard dependency of your mod simply use `CurrentTime.Time` whenever you need the current time

If DvTime should only be a soft dependency of your mod add this class to your mod and use `TimeSource.GetCurrentTime()`
```cs
public static class TimeSource
{
  static TimeSource()
  {
    try
    {
      // separate function to be able to catch dll load exceptions when DvTime is not installed
      DoInitialize();
    }
    catch (Exception ex)
    {
      Debug.Log($"unable to load DvTime: {ex}" );
    }
    
    if (GetCurrentTime is null)
    {
      // Implement some kind of fallback strategy here
      GetCurrentTime = () => DateTime.Now;
    }
  }

  private static void DoInitialize()
  {
    // Get once to trigger dll load
    _ = CurrentTime.Time;
    GetCurrentTime = () => CurrentTime.Time;
  }

  public static Func<DateTime> GetCurrentTime;
}
```

## Adding new sources
Implement the interface `RedworkDE.DvTime.ITimeSource`.  
Add an event handler to `RedworkDE.DvTime.TimeUpdater.RegisterTimeSource` and in the event handler add your new source to the provided list.  
The first item in that list will be used as the default source.
