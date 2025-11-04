using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace JpegViewer.App.UI.Support
{
    /// <summary>
    /// Class to implement and do some animation on given UI elements.
    /// </summary>
    public class AnimationHelper
    {
        /// <summary>
        /// Holds the spring animation used for scaling on hover.
        /// </summary>
        public SpringVector3NaturalMotionAnimation SpringAnimation { get; }

        /// <summary>
        /// Holds the storyboard for color animation.
        /// </summary>
        public Storyboard Storyboard { get; }

        /// <summary>
        /// Holds the color animation for hover effect.
        /// </summary>
        public ColorAnimation HoverColorAnimation { get; }

        /// <summary>
        /// Up scale vector value.
        /// </summary>
        public Vector3 UpScaleVector { get; } = new Vector3(1.05f);

        /// <summary>
        /// Down scale vector value.
        /// </summary>
        public Vector3 NormalScaleVector { get; } = new Vector3(1.00f);

        /// <summary>
        /// Pressed state scale vector value.
        /// </summary>
        public Vector3 PressedScaleVector { get; } = new Vector3(0.95f);

        /// <summary>
        /// Map with the supported target types
        /// </summary>
        private static Dictionary<Type, string> TargetMap { get; } = new Dictionary<Type, string>()
        {
            { typeof(FontIcon), "(FontIcon.Foreground).(SolidColorBrush.Color)" },
            { typeof(TextBlock), "(TextBlock.Foreground).(SolidColorBrush.Color)" }
        };

        public AnimationHelper()
        {
            // Hover scale spring animation setup
            SpringAnimation = CompositionTarget.GetCompositorForCurrentThread().CreateSpringVector3Animation();
            SpringAnimation.DampingRatio = 0.2f;
            SpringAnimation.Target = "Scale";

            // Hover color animation setup
            Storyboard = new Storyboard() { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever };
            HoverColorAnimation = new ColorAnimation() { To = Colors.Red, Duration = new Duration(TimeSpan.FromMilliseconds(450)) };
            Storyboard.Children.Add(HoverColorAnimation);
        }

        /// <summary>
        /// Animation on PointerEntered event on a parent and its child item.
        /// </summary>
        public void DoAnimationPointerEntered(object sender, object child, Type childType)
        {
            // Do Scale animation
            SpringAnimation.FinalValue = UpScaleVector;
            (sender as UIElement)!.StartAnimation(SpringAnimation);

            // Do Color animation
            Storyboard.Stop();
            Storyboard.SetTarget(HoverColorAnimation, (DependencyObject)Convert.ChangeType(child, childType));
            if (TargetMap.ContainsKey(childType))
            {
                Storyboard.SetTargetProperty(HoverColorAnimation, TargetMap[childType]);
            }
            Storyboard.Begin();
        }

        /// <summary>
        /// Animation on PointerEntered event.
        /// </summary>
        public void DoAnimationPointerEntered(object sender, Type senderType)
        {
            // Do Scale animation
            SpringAnimation.FinalValue = UpScaleVector;
            (sender as UIElement)!.StartAnimation(SpringAnimation);

            // Do Color animation
            Storyboard.Stop();
            Storyboard.SetTarget(HoverColorAnimation, (DependencyObject)Convert.ChangeType(sender, senderType));
            if (TargetMap.ContainsKey(senderType))
            {
                Storyboard.SetTargetProperty(HoverColorAnimation, TargetMap[senderType]);
            }
            Storyboard.Begin();
        }

        /// <summary>
        /// Animation on PointerExited event.
        /// </summary>
        public void DoAnimationPointerExited(object sender)
        {
            // Stop Color animation
            Storyboard.Stop();

            // Do Scale animation back to normal
            SpringAnimation.FinalValue = NormalScaleVector;
            (sender as UIElement)!.StartAnimation(SpringAnimation);
        }

        /// <summary>
        /// Scale animation of button pressed state.
        /// </summary>
        public void DoAnimationPointerPressed(object sender)
        {
            // Do Scale animation to pressed
            SpringAnimation.FinalValue = PressedScaleVector;
            (sender as UIElement)!.StartAnimation(SpringAnimation); 
        }

        /// <summary>
        /// Scale animation of button released state.
        /// </summary>
        public void DoAnimationPointerReleased(object sender)
        {
            // Do Scale animation back to normal
            SpringAnimation.FinalValue = NormalScaleVector;
            (sender as UIElement)!.StartAnimation(SpringAnimation);
        }
    }
}
