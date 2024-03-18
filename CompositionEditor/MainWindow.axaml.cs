using System;
using System.Numerics;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using Vector = Avalonia.Vector;

namespace CompositionEditor;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private Vector3D _offset;
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        Panel.AddHandler(Gestures.PullGestureEvent, OnPullGesture);
        Panel.AddHandler(Gestures.PullGestureEndedEvent, PullGestureEnded);

        var compositionVisual = ElementComposition.GetElementVisual(Panel);
        _offset = compositionVisual.Offset;
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        
        Panel.RemoveHandler(Gestures.PullGestureEvent, OnPullGesture);
        Panel.RemoveHandler(Gestures.PullGestureEndedEvent, PullGestureEnded);
    }

    private void Update(Vector eDelta)
    {
        var compositionVisual = ElementComposition.GetElementVisual(Panel);

        var compositor = compositionVisual.Compositor;

        compositionVisual.Offset = _offset + new Vector3D(0, eDelta.Y, 0);
        Console.WriteLine($"Offset={compositionVisual.Offset}");
        // compositionVisual.Offset = new Vector3D(0, 0, 0);
        // compositionVisual.Opacity = 0.6f;

        //Panel.InvalidateMeasure();
        //Panel.InvalidateVisual();
    }

    private void OnPullGesture(object? sender, PullGestureEventArgs e)
    {
        switch (e.PullDirection)
        {
            case PullDirection.LeftToRight:
                break;
            case PullDirection.RightToLeft:
                break;
            case PullDirection.TopToBottom:
                Console.WriteLine($"Delta={e.Delta}");
                Update(e.Delta);
                e.Handled = true;
                break;
            case PullDirection.BottomToTop:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void PullGestureEnded(object? sender, PullGestureEndedEventArgs e)
    {
        switch (e.PullDirection)
        {
            case PullDirection.LeftToRight:
                break;
            case PullDirection.RightToLeft:
                break;
            case PullDirection.TopToBottom:
                //Update(new Vector());
                
                var compositionVisual = ElementComposition.GetElementVisual(Panel);

                var compositor = compositionVisual.Compositor;
                
                var easing = new SpringEasing(1, 2000, 35);

                var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
                offsetAnimation.InsertKeyFrame(0.0f, new Vector3((float)compositionVisual.Offset.X, (float)compositionVisual.Offset.Y, 0), easing);
                offsetAnimation.InsertKeyFrame(1.0f, new Vector3((float)_offset.X, (float)_offset.Y, 0), easing);
                offsetAnimation.Direction = PlaybackDirection.Normal;
                offsetAnimation.Duration = TimeSpan.FromMilliseconds(800);
                offsetAnimation.IterationBehavior = AnimationIterationBehavior.Count;
                offsetAnimation.IterationCount = 1;

                compositionVisual.StartAnimation("Offset", offsetAnimation);
                
                break;
            case PullDirection.BottomToTop:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

}
