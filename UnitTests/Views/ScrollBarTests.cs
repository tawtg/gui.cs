﻿using Xunit.Abstractions;
using static Unix.Terminal.Delegates;

namespace Terminal.Gui.ViewsTests;

public class ScrollBarTests
{
    public ScrollBarTests (ITestOutputHelper output) { _output = output; }
    private readonly ITestOutputHelper _output;

    [Fact]
    [AutoInitShutdown]
    public void AutoHideScrollBar_CheckScrollBarVisibility ()
    {
        var scrollBar = new ScrollBar { Width = 2, Height = Dim.Fill (), Size = 30 };
        View scrollBarSuperView = ScrollBarSuperView ();
        scrollBarSuperView.Add (scrollBar);
        Application.Begin ((scrollBarSuperView.SuperView as Toplevel)!);

        Assert.Equal (Orientation.Vertical, scrollBar.Orientation);
        //Assert.True (scrollBar.ShowScrollIndicator);
        Assert.True (scrollBar.Visible);
        Assert.Equal ("Absolute(2)", scrollBar.Width!.ToString ());
        Assert.Equal (2, scrollBar.Viewport.Width);
        Assert.Equal ("Fill(Absolute(0))", scrollBar.Height!.ToString ());
        Assert.Equal (25, scrollBar.Viewport.Height);

        scrollBar.Size = 10;
        //Assert.False (scrollBar.ShowScrollIndicator);
        Assert.False (scrollBar.Visible);

        scrollBar.Size = 30;
        //Assert.True (scrollBar.ShowScrollIndicator);
        Assert.True (scrollBar.Visible);

        scrollBar.AutoHide = false;
        //Assert.True (scrollBar.ShowScrollIndicator);
        Assert.True (scrollBar.Visible);

        scrollBar.Size = 10;
        //Assert.True (scrollBar.ShowScrollIndicator);
        Assert.True (scrollBar.Visible);

        scrollBarSuperView.SuperView!.Dispose ();
    }


    [Fact]
    public void Constructor_Defaults ()
    {
        var scrollBar = new ScrollBar ();
        Assert.False (scrollBar.CanFocus);
        Assert.Equal (Orientation.Vertical, scrollBar.Orientation);
        Assert.Equal (0, scrollBar.Size);
        Assert.Equal (0, scrollBar.GetSliderPosition ());
        Assert.True (scrollBar.AutoHide);
    }


    [Fact]
    public void OnOrientationChanged_Keeps_Size ()
    {
        var scroll = new ScrollBar ();
        scroll.Layout ();
        scroll.Size = 1;

        scroll.Orientation = Orientation.Horizontal;
        Assert.Equal (1, scroll.Size);
    }

    [Fact]
    public void OnOrientationChanged_Sets_Position_To_0 ()
    {
        View super = new View ()
        {
            Id = "super",
            Width = 10,
            Height = 10
        };
        var scrollBar = new ScrollBar ()
        {
        };
        super.Add (scrollBar);
        scrollBar.Layout ();
        scrollBar.ContentPosition = 1;
        scrollBar.Orientation = Orientation.Horizontal;

        Assert.Equal (0, scrollBar.GetSliderPosition ());
    }


    [Fact]
    public void ContentPosition_Event_Cancelables ()
    {
        var changingCount = 0;
        var changedCount = 0;
        var scrollBar = new ScrollBar { };
        scrollBar.Layout ();
        scrollBar.Size = scrollBar.Viewport.Height * 2;
        scrollBar.Layout ();

        scrollBar.ContentPositionChanging += (s, e) =>
                                            {
                                                if (changingCount == 0)
                                                {
                                                    e.Cancel = true;
                                                }

                                                changingCount++;
                                            };
        scrollBar.ContentPositionChanged += (s, e) => changedCount++;

        scrollBar.ContentPosition = 1;
        Assert.Equal (0, scrollBar.ContentPosition);
        Assert.Equal (1, changingCount);
        Assert.Equal (0, changedCount);

        scrollBar.ContentPosition = 1;
        Assert.Equal (1, scrollBar.ContentPosition);
        Assert.Equal (2, changingCount);
        Assert.Equal (1, changedCount);
    }



    [Fact (Skip = "Disabled - Will put this feature in View")]
    [AutoInitShutdown]
    public void KeepContentInAllViewport_True_False ()
    {
        var view = new View { Width = Dim.Fill (), Height = Dim.Fill () };
        view.Padding.Thickness = new (0, 0, 2, 0);
        view.SetContentSize (new (view.Viewport.Width, 30));
        var scrollBar = new ScrollBar { Width = 2, Height = Dim.Fill (), Size = view.GetContentSize ().Height };
        scrollBar.SliderPositionChanged += (_, e) => view.Viewport = view.Viewport with { Y = e.CurrentValue };
        view.Padding.Add (scrollBar);
        var top = new Toplevel ();
        top.Add (view);
        Application.Begin (top);

        Assert.False (scrollBar.KeepContentInAllViewport);
        scrollBar.KeepContentInAllViewport = true;
        Assert.Equal (80, view.Padding.Viewport.Width);
        Assert.Equal (25, view.Padding.Viewport.Height);
        Assert.Equal (2, scrollBar.Viewport.Width);
        Assert.Equal (25, scrollBar.Viewport.Height);
        Assert.Equal (30, scrollBar.Size);

        scrollBar.KeepContentInAllViewport = false;
        scrollBar.ContentPosition = 50;
        Assert.Equal (scrollBar.GetSliderPosition (), scrollBar.Size - 1);
        Assert.Equal (scrollBar.GetSliderPosition (), view.Viewport.Y);
        Assert.Equal (29, scrollBar.GetSliderPosition ());
        Assert.Equal (29, view.Viewport.Y);

        top.Dispose ();
    }


    [Fact]
    public void Size_Cannot_Be_Negative ()
    {
        var scrollBar = new ScrollBar { Height = 10, Size = -1 };
        Assert.Equal (0, scrollBar.Size);
        scrollBar.Size = -10;
        Assert.Equal (0, scrollBar.Size);
    }

    [Fact]
    public void SizeChanged_Event ()
    {
        var count = 0;
        var scrollBar = new ScrollBar ();
        scrollBar.SizeChanged += (s, e) => count++;

        scrollBar.Size = 10;
        Assert.Equal (10, scrollBar.Size);
        Assert.Equal (1, count);
    }

    [Theory]
    [SetupFakeDriver]

    #region Vertical

    #region Super 10 - ScrollBar 8
    [InlineData (
                    10,
                    1,
                    10,
                    -1,
                    Orientation.Horizontal,
                    @"
┌──────────┐
│◄████████►│
└──────────┘")]

    [InlineData (
                    10,
                    1,
                    20,
                    -1,
                    Orientation.Horizontal,
                    @"
┌──────────┐
│◄████░░░░►│
└──────────┘")]
    [InlineData (
                    10,
                    1,
                    20,
                    0,
                    Orientation.Horizontal,
                    @"
┌──────────┐
│◄████░░░░►│
└──────────┘")]

    [InlineData (
                    10,
                    1,
                    20,
                    1,
                    Orientation.Horizontal,
                    @"
┌──────────┐
│◄████░░░░►│
└──────────┘")]

    [InlineData (
                    10,
                    1,
                    20,
                    2,
                    Orientation.Horizontal,
                    @"
┌──────────┐
│◄░████░░░►│
└──────────┘
")]

    [InlineData (
                    10,
                    1,
                    20,
                    3,
                    Orientation.Horizontal,
                    @"
┌──────────┐
│◄░████░░░►│
└──────────┘
")]

    [InlineData (
                    10,
                    1,
                    20,
                    4,
                    Orientation.Horizontal,
                    @"
┌──────────┐
│◄░░████░░►│
└──────────┘
")]
    [InlineData (
                    10,
                    1,
                    20,
                    5,
                    Orientation.Horizontal,
                    @"
┌──────────┐
│◄░░████░░►│
└──────────┘
")]

    [InlineData (
                    10,
                    1,
                    20,
                    6,
                    Orientation.Horizontal,
                    @"
┌──────────┐
│◄░░░████░►│
└──────────┘
")]

    [InlineData (
                    10,
                    1,
                    20,
                    7,
                    Orientation.Horizontal,
                    @"
┌──────────┐
│◄░░░████░►│
└──────────┘
")]


    [InlineData (
                    10,
                    1,
                    20,
                    8,
                    Orientation.Horizontal,
                    @"
┌──────────┐
│◄░░░░████►│
└──────────┘
")]

    [InlineData (
                    10,
                    1,
                    20,
                    9,
                    Orientation.Horizontal,
                    @"
┌──────────┐
│◄░░░░████►│
└──────────┘
")]

    [InlineData (
                    10,
                    1,
                    20,
                    10,
                    Orientation.Horizontal,
                    @"
┌──────────┐
│◄░░░░████►│
└──────────┘
")]


    [InlineData (
                    10,
                    1,
                    20,
                    19,
                    Orientation.Horizontal,
                    @"
┌──────────┐
│◄░░░░████►│
└──────────┘
")]


    [InlineData (
                    10,
                    1,
                    20,
                    20,
                    Orientation.Horizontal,
                    @"
┌──────────┐
│◄░░░░████►│
└──────────┘
")]
    #endregion  Super 10 - ScrollBar 8

    #region  Super 12 - ScrollBar 10
    [InlineData (
                    12,
                    1,
                    10,
                    -1,
                    Orientation.Horizontal,
                    @"
┌────────────┐
│◄██████████►│
└────────────┘")]

    [InlineData (
                    12,
                    1,
                    20,
                    -1,
                    Orientation.Horizontal,
                    @"
┌────────────┐
│◄██████░░░░►│
└────────────┘")]
    [InlineData (
                    12,
                    1,
                    20,
                    0,
                    Orientation.Horizontal,
                    @"
┌────────────┐
│◄██████░░░░►│
└────────────┘")]

    [InlineData (
                    12,
                    1,
                    20,
                    1,
                    Orientation.Horizontal,
                    @"
┌────────────┐
│◄██████░░░░►│
└────────────┘")]

    [InlineData (
                    12,
                    1,
                    20,
                    2,
                    Orientation.Horizontal,
                    @"
┌────────────┐
│◄░██████░░░►│
└────────────┘
")]

    [InlineData (
                    12,
                    1,
                    20,
                    3,
                    Orientation.Horizontal,
                    @"
┌────────────┐
│◄░██████░░░►│
└────────────┘
")]

    [InlineData (
                    12,
                    1,
                    20,
                    4,
                    Orientation.Horizontal,
                    @"
┌────────────┐
│◄░░██████░░►│
└────────────┘
")]
    [InlineData (
                    12,
                    1,
                    20,
                    5,
                    Orientation.Horizontal,
                    @"
┌────────────┐
│◄░░░██████░►│
└────────────┘
")]

    [InlineData (
                    12,
                    1,
                    20,
                    6,
                    Orientation.Horizontal,
                    @"
┌────────────┐
│◄░░░██████░►│
└────────────┘
")]

    [InlineData (
                    12,
                    1,
                    20,
                    7,
                    Orientation.Horizontal,
                    @"
┌────────────┐
│◄░░░░██████►│
└────────────┘
")]


    [InlineData (
                    12,
                    1,
                    20,
                    8,
                    Orientation.Horizontal,
                    @"
┌────────────┐
│◄░░░░██████►│
└────────────┘
")]

    [InlineData (
                    12,
                    1,
                    20,
                    9,
                    Orientation.Horizontal,
                    @"
┌────────────┐
│◄░░░░██████►│
└────────────┘
")]

    [InlineData (
                    12,
                    1,
                    20,
                    10,
                    Orientation.Horizontal,
                    @"
┌────────────┐
│◄░░░░██████►│
└────────────┘
")]


    [InlineData (
                    12,
                    1,
                    20,
                    19,
                    Orientation.Horizontal,
                    @"
┌────────────┐
│◄░░░░██████►│
└────────────┘
")]


    [InlineData (
                    12,
                    1,
                    20,
                    20,
                    Orientation.Horizontal,
                    @"
┌────────────┐
│◄░░░░██████►│
└────────────┘
")]
    #endregion Super 12 - ScrollBar 10
    [InlineData (
                    10,
                    3,
                    20,
                    2,
                    Orientation.Horizontal,
                    @"
┌──────────┐
│ ░████░░░ │
│◄░████░░░►│
│ ░████░░░ │
└──────────┘
")]
    #endregion Vertical

    #region Horizontal

    [InlineData (
                    1,
                    10,
                    10,
                    -1,
                    Orientation.Vertical,
                    @"
┌─┐
│▲│
│█│
│█│
│█│
│█│
│█│
│█│
│█│
│█│
│▼│
└─┘")]

    [InlineData (
                    1,
                    10,
                    10,
                    5,
                    Orientation.Vertical,
                    @"
┌─┐
│▲│
│█│
│█│
│█│
│█│
│█│
│█│
│█│
│█│
│▼│
└─┘")]

    [InlineData (
                    1,
                    10,
                    20,
                    5,
                    Orientation.Vertical,
                    @"
┌─┐
│▲│
│░│
│░│
│█│
│█│
│█│
│█│
│░│
│░│
│▼│
└─┘")]

    [InlineData (
                    1,
                    12,
                    20,
                    5,
                    Orientation.Vertical,
                    @"
┌─┐
│▲│
│░│
│░│
│░│
│█│
│█│
│█│
│█│
│█│
│█│
│░│
│▼│
└─┘")]

    [InlineData (
                    3,
                    10,
                    20,
                    2,
                    Orientation.Vertical,
                    @"
┌───┐
│ ▲ │
│░░░│
│███│
│███│
│███│
│███│
│░░░│
│░░░│
│░░░│
│ ▼ │
└───┘
")]
    #endregion


    public void Draws_Correctly (int superWidth, int superHeight, int contentSize, int contentPosition, Orientation orientation, string expected)
    {
        var super = new Window
        {
            Id = "super",
            Width = superWidth + 2,
            Height = superHeight + 2
        };

        var scrollBar = new ScrollBar
        {
            Orientation = orientation,
        };

        if (orientation == Orientation.Vertical)
        {
            scrollBar.Width = Dim.Fill ();
        }
        else
        {
            scrollBar.Height = Dim.Fill ();
        }
        super.Add (scrollBar);

        scrollBar.Size = contentSize;
        scrollBar.Layout ();
        scrollBar.ContentPosition = contentPosition;

        super.BeginInit ();
        super.EndInit ();
        super.Layout ();
        super.Draw ();

        _ = TestHelpers.AssertDriverContentsWithFrameAre (expected, _output);
    }

    private View ScrollBarSuperView ()
    {
        var view = new View
        {
            Width = Dim.Fill (),
            Height = Dim.Fill ()
        };

        var top = new Toplevel ();
        top.Add (view);

        return view;
    }


    [Theory]
    [CombinatorialData]
    [AutoInitShutdown]
    public void Mouse_Click_DecrementButton_Decrements ([CombinatorialRange (1, 3, 1)] int increment, Orientation orientation)
    {
        var top = new Toplevel ()
        {
            Id = "top",
            Width = 10,
            Height = 10
        };
        var scrollBar = new ScrollBar
        {
            Id = "scrollBar",
            Orientation = orientation,
            Size = 20,
            Increment = increment
        };

        top.Add (scrollBar);
        RunState rs = Application.Begin (top);
        scrollBar.ContentPosition = 5;
        Application.RunIteration (ref rs);

        Assert.Equal (5, scrollBar.GetSliderPosition ());
        Assert.Equal (12, scrollBar.ContentPosition);
        int initialPos = scrollBar.ContentPosition;

        Application.RaiseMouseEvent (new ()
        {
            ScreenPosition = new (0, 0),
            Flags = MouseFlags.Button1Clicked
        });
        Application.RunIteration (ref rs);

        Assert.Equal (initialPos - increment, scrollBar.ContentPosition);

        Application.ResetState (true);
    }


    [Theory]
    [CombinatorialData]
    [AutoInitShutdown]
    public void Mouse_Click_IncrementButton_Increments ([CombinatorialRange (1, 3, 1)] int increment, Orientation orientation)
    {
        var top = new Toplevel ()
        {
            Id = "top",
            Width = 10,
            Height = 10
        };
        var scrollBar = new ScrollBar
        {
            Id = "scrollBar",
            Orientation = orientation,
            Size = 20,
            Increment = increment
        };

        top.Add (scrollBar);
        RunState rs = Application.Begin (top);
        scrollBar.ContentPosition = 0;
        Application.RunIteration (ref rs);

        Assert.Equal (0, scrollBar.GetSliderPosition ());
        Assert.Equal (0, scrollBar.ContentPosition);
        int initialPos = scrollBar.ContentPosition;

        Application.RaiseMouseEvent (new ()
        {
            ScreenPosition = orientation == Orientation.Vertical ? new (0, scrollBar.Frame.Height - 1) : new (scrollBar.Frame.Width - 1, 0),
            Flags = MouseFlags.Button1Clicked
        });
        Application.RunIteration (ref rs);

        Assert.Equal (initialPos + increment, scrollBar.ContentPosition);

        Application.ResetState (true);
    }

    [Theory]
    [InlineData (1, 10, 0, 0)]
    [InlineData (1, 10, 5, 0)]
    [InlineData (1, 10, 10, 1)]
    [InlineData (1, 20, 0, 0)]
    [InlineData (1, 20, 10, 0)]
    [InlineData (1, 20, 20, 1)]
    [InlineData (2, 10, 0, 0)]
    [InlineData (2, 10, 5, 1)]
    [InlineData (2, 10, 10, 2)]
    [InlineData (2, 20, 0, 0)]
    [InlineData (2, 20, 10, 1)]
    [InlineData (2, 20, 20, 2)]
    [InlineData (3, 10, 0, 0)]
    [InlineData (3, 10, 5, 1)]
    [InlineData (3, 10, 10, 3)]
    [InlineData (3, 20, 0, 0)]
    [InlineData (3, 20, 10, 1)]
    [InlineData (3, 20, 20, 3)]
    [InlineData (4, 10, 0, 0)]
    [InlineData (4, 10, 5, 2)]
    [InlineData (4, 10, 10, 4)]
    [InlineData (4, 20, 0, 0)]
    [InlineData (4, 20, 10, 2)]
    [InlineData (4, 20, 20, 4)]
    [InlineData (5, 10, 0, 0)]
    [InlineData (5, 10, 5, 2)]
    [InlineData (5, 10, 10, 5)]
    [InlineData (5, 20, 0, 0)]
    [InlineData (5, 20, 10, 2)]
    [InlineData (5, 20, 20, 5)]
    public void ScrollBar_CombinatorialTests (int width, int size, int contentPosition, int expectedSliderPosition)
    {
        // Arrange
        var scrollBar = new ScrollBar
        {
            Width = width,
            Size = size,
            ContentPosition = contentPosition
        };
        scrollBar.Layout ();

        // Act
        var sliderPosition = scrollBar.GetSliderPosition ();

        // Assert
        Assert.Equal (expectedSliderPosition, sliderPosition);
    }

}
