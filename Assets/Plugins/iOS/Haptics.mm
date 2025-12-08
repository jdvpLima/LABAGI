#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

extern "C" void _TriggerLightHaptic()
{
    if (@available(iOS 10.0, *))
    {
        UIImpactFeedbackGenerator* generator = 
            [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
        [generator impactOccurred];
    }
}
