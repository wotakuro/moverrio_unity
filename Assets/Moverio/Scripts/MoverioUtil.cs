/**
Copyright (c) 2015 Yusuke Kurokawa

Permission is hereby granted, free of charge, to any person obtaining a 
copy of this software and associated documentation files (the 
"Software"), to deal in the Software without restriction, including 
without limitation the rights to use, copy, modify, merge, publish, 
distribute, sublicense, and/or sell copies of the Software, and to 
permit persons to whom the Software is furnished to do so, subject to 
the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION 
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using UnityEngine;
using System.Collections;

/// <summary>
/// Moverio native utility
/// </summary>
public class MoverioUtil
{

    // ------------ const -------------//
    /// 2D mode
    public const int DISPLAY_MODE_2D = 0;
    /// 3D mode 
    public const int DISPLAY_MODE_3D = 1;
    /// get data from glass sensors
    public const int SENSOR_MODE_GLASS = 0;
    /// get data from controller sensors 
    public const int SENSOR_MODE_CONTROLLER = 1;

    /// android class
    private const string ANDROID_CLASS = "com.wotakuro.moverio.utils.MoverioUtils";
    // ------------ vars -----------------//


    // ----------- functions ------------//

    /// <summary>
    /// private constructor.
    /// This shouldn't instansiated
    /// </summary>
    private MoverioUtil()
    {
    }

    /// <summary>
    /// change Display mode
    /// </summary>
    /// <param name="mode">set DISPLAY_MODE_2D or DISPLAY_MODE_3D</param>
    public static void setDisplayMode(int mode)
    {
        AndroidJavaClass jc = new AndroidJavaClass(ANDROID_CLASS);
        jc.CallStatic("setDisplayMode", mode);
    }

    /// <summary>
    /// change Sensor mode
    /// </summary>
    /// <param name="mode">set SENSOR_MODE_GLASS or SENSOR_MODE_CONTROLLER</param>
    public static void setSensorMode(int mode)
    {
        AndroidJavaClass jc = new AndroidJavaClass(ANDROID_CLASS);
        jc.CallStatic("setSensorMode", mode);
    }

}
