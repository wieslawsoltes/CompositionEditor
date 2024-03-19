using System;
using System.Collections.Generic;
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

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        AddHandler(Gestures.PullGestureEvent, OnPullGesture);
        AddHandler(Gestures.PullGestureEndedEvent, PullGestureEnded);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        
        RemoveHandler(Gestures.PullGestureEvent, OnPullGesture);
        RemoveHandler(Gestures.PullGestureEndedEvent, PullGestureEnded);
    }

    private void Update(Control control, Vector eDelta, Vector3D originalOffset)
    {
        var compositionVisual = ElementComposition.GetElementVisual(control);

        var compositor = compositionVisual.Compositor;

        compositionVisual.Offset = originalOffset + new Vector3D(eDelta.X, eDelta.Y, 0);
        // compositionVisual.Offset = new Vector3D(0, 0, 0);
        // compositionVisual.Opacity = 0.6f;

        // Console.WriteLine($"Offset={compositionVisual.Offset}");
    }

    private Dictionary<Control, Vector3D> _originalOffset = new Dictionary<Control, Vector3D>();
    
    private void OnPullGesture(object? sender, PullGestureEventArgs e)
    {
        // Console.WriteLine($"Delta={e.Delta}");

        var control = e.Source as Control;

        if (!_originalOffset.ContainsKey(control))
        {
            var compositionVisual = ElementComposition.GetElementVisual(control);
            _originalOffset[control] = compositionVisual.Offset;
        }

        switch (e.PullDirection)
        {
            case PullDirection.LeftToRight:
            {
                if (control != Panel)
                {
                    return;
                }

                Update(control, new Vector(e.Delta.X, 0), _originalOffset[control]);
                e.Handled = true;
                break;
            }
            case PullDirection.RightToLeft:
            {
                if (control == Panel)
                {
                    return;
                }
                
                Update(control, new Vector(-e.Delta.X, 0), _originalOffset[control]);
                e.Handled = true;
                break;
            }
            case PullDirection.TopToBottom:
            {
                if (control != Panel)
                {
                    return;
                }

                Update(control, new Vector(0, e.Delta.Y), _originalOffset[control]);
                e.Handled = true;
                break;
            }
            case PullDirection.BottomToTop:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void PullGestureEnded(object? sender, PullGestureEndedEventArgs e)
    {
        var control = e.Source as Control;

        if (!_originalOffset.ContainsKey(control))
        {
            return;
        }

        switch (e.PullDirection)
        {
            case PullDirection.LeftToRight:
                if (control != Panel)
                {
                    return;
                }
                StartOffsetAnimation(control, _originalOffset[control]);
                e.Handled = true;
                break;
            case PullDirection.RightToLeft:
                if (control == Panel)
                {
                    return;
                }
                StartOffsetAnimation(control, _originalOffset[control]);
                e.Handled = true;
                break;
            case PullDirection.TopToBottom:
                if (control != Panel)
                {
                    return;
                }
                StartOffsetAnimation(control, _originalOffset[control]);
                e.Handled = true;
                break;
            case PullDirection.BottomToTop:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void StartOffsetAnimation(Control control, Vector3D offset)
    {
        var compositionVisual = ElementComposition.GetElementVisual(control);

        var compositor = compositionVisual.Compositor;

        var easing1 = new SpringEasing(1, 2000, 35);
        var easing2 = new SpringEasing(1, 4000, 50);

        var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
        offsetAnimation.InsertKeyFrame(0.0f, new Vector3((float)compositionVisual.Offset.X, (float)compositionVisual.Offset.Y, 0), easing1);
        offsetAnimation.InsertKeyFrame(1.0f, new Vector3((float)offset.X, (float)offset.Y, 0), easing2);
        offsetAnimation.Direction = PlaybackDirection.Normal;
        offsetAnimation.Duration = TimeSpan.FromMilliseconds(800);
        offsetAnimation.IterationBehavior = AnimationIterationBehavior.Count;
        offsetAnimation.IterationCount = 1;

        compositionVisual.StartAnimation("Offset", offsetAnimation);
    }
}
