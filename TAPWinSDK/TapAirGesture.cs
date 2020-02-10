using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAPWin
{
    public enum TAPAirGesture : int
    {
        Undefined = -1000,
        OneFingerUp = 2,
        TwoFingersUp = 3,
        OnefingerDown = 4,
        TwoFingersDown = 5,
        OneFingerLeft = 6,
        TwoFingersLeft = 7,
        OneFingerRight = 8,
        TwoFingersRight = 9,
        IndexToThumbTouch = 1000,
        MiddleToThumbTouch = 1001
    }

    internal class TAPAirGestureHelper
    {
        private TAPAirGestureHelper()
        {

        }

        internal static TAPAirGesture tapToAirGesture(int combination)
        {
            if (combination == 2)
            {
                return TAPAirGesture.IndexToThumbTouch;
            } else if (combination == 4)
            {
                return TAPAirGesture.MiddleToThumbTouch;
            }
            return TAPAirGesture.Undefined;
        }

        internal static TAPAirGesture intToAirGesture(int code)
        {
            if (Enum.IsDefined(typeof(TAPAirGesture),code))
            {
                return (TAPAirGesture)code;
            } else
            {
                return TAPAirGesture.Undefined;
            }
        }
    }
}
