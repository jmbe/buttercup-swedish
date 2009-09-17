using System;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;

namespace Control.UI.Gestures
{
    public class MouseGestures
    {
		#region Fields (10) 

        private UIElement _container;
        private bool _enableComplexGestures = true;
        private bool _enabled = true;
        private Point _gestureStartLocation;
        private Point _lastPoint;
        /// <summary>
        /// Maximal ratio of unknown MouseMuveSegments it the gesture
        /// </summary>
        private const float _maxUnknownSkipRatio = 0.35F;
        private int _minGestureSize = 30;
        /// <summary>
        /// Minimal length of MouseMoveSegment
        /// </summary>
        private const uint _mouseMoveSegmentLength = 8;
        private List<MouseMoveSegment> _mouseMoveSegments;
        
        private bool _working;
        private GestureState _state = GestureState.Stopped;

		#endregion Fields 

		#region Constructors (2) 

        /// <summary>
        /// Creates MouseGestures component
        /// </summary>
        /// <param name="useLLMessageFilter">Specifies whether use LLMessageFilter</param>
        public MouseGestures(UIElement element)
        {
            _mouseMoveSegments = new List<MouseMoveSegment>();

            //element.Enabled = enabled;

            _container = element;

            element.MouseLeftButtonDown += new MouseButtonEventHandler(BeginGesture);
            element.MouseMove += new MouseEventHandler(AddToGesture);
            element.MouseLeftButtonUp += new MouseButtonEventHandler(EndGesture);
        }

        ~MouseGestures()
        {

        }

		#endregion Constructors 

		#region Properties (5) 

        public UIElement Container
        {
            get 
            { 
                return _container; 
            }
            set 
            {
                _container = value; 
            }
        }

        /// <summary>
        /// Gets or sets propery indicating whether component should recognize
        /// complex gestures
        /// </summary>
        public bool EnableComplexGestures
        {
            get
            {
                return _enableComplexGestures;
            }
            set
            {
                _enableComplexGestures = value;
            }
        }

        /// <summary>
        /// Gets or sets propery indicating whether component is enabled and
        /// will recognize mouse gestures
        /// </summary>
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;

            }
        }

        /// <summary>
        /// Gets or sets minimal gesture size in pixels
        /// </summary>
        public int MinGestureSize
        {
            get
            {
                return _minGestureSize;
            }
            set
            {
                if (value > 0)
                    _minGestureSize = value;
            }
        }

        /// <summary>
        /// Gets value indicating whether component is capturing and recognizing
        /// mouse gesture
        /// </summary>
        public bool Working
        {
            get
            {
                return _working;
            }
        }

        public GestureState State
        {
            set
            {
                _state = value;
            }
            get
            {
                return _state;
            }
        }

		#endregion Properties 

		#region Delegates and Events (2) 

		// Delegates (1) 

        /// <summary>
        /// Represents the method that will handle MouseGesture events.
        /// </summary>
        /// <param name="sender">The source of event.</param>
        /// <param name="start">A MouseGestureEventArgs that contains event data.</param>
        public delegate void GestureHandler(object sender, MouseGestureEventArgs e);
		// Events (1) 

        public event GestureHandler GestureRaised;

		#endregion Delegates and Events 

		#region Methods (11) 

		// Public Methods (3) 

        /// <summary>
        /// Adds MouseMoveSegment to the current gesture.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Mouse event data</param>
        /// <remarks>
        /// Function is called on the MouseMoveSegment of MouseMessageFilter
        /// The segment is added only when segment length is greater then 
        /// mouseMoveSegmentLength
        /// </remarks>
        public void AddToGesture(object sender, MouseEventArgs e)
        {
            if (State == GestureState.Initiated || State == GestureState.InProgress)
            //{
                if (GetDistance(_lastPoint, e.GetPosition(Container)) >= _mouseMoveSegmentLength)
                {
                    //_working = true;
                   State = GestureState.InProgress; 
                    MouseMoveSegment segment = new MouseMoveSegment(_lastPoint, e.GetPosition(Container));
                    _mouseMoveSegments.Add(segment);
                    _lastPoint = e.GetPosition(Container);
                }
            //}

        }

        /// <summary>
        /// Starts new mouse gesture
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Mouse event data</param>
        /// <remarks>
        /// Functions is called on the RightButtonDown event of MouseMessageFilter.
        /// </remarks>
        public void BeginGesture(object sender, MouseButtonEventArgs e)
        {
            _mouseMoveSegments.Clear();

            //_working = true;
            State = GestureState.Initiated;

            _gestureStartLocation = e.GetPosition(Container);
            _lastPoint = e.GetPosition(Container);
        }

        /// <summary>
        /// Stops mouse gesture recording and tries to recognize the gesture
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Mouse event data</param>
        public void EndGesture(object sender, MouseButtonEventArgs e)
        {
            //_working = false;
            State = GestureState.Stopped;

            /*
            //check minimal length
            //TODO change minimal length checking  - does not work for gesture LeftRight, etc...
            if (mouseMoveSegments.Count * mouseMoveSegmentLength < minGestureSize)
            {
                //too short for mouse gesture - send regular right mouse click
                mf.Enabled = false;
                mf.Enabled = true;

                return;
            }
             * */

            //try recognize mouse gesture
            MouseGesture gesture = RecognizeGesture();
            if (gesture != MouseGesture.Unknown)
            {
                RaiseGestureEvents(gesture);
            }

        }
		// Protected Methods (2) 

        /// <summary>
        /// Recognizes complex MouseGesture from two simple gestures
        /// </summary>
        /// <param name="firstGesture">First simple gesture.</param>
        /// <param name="secondGesture">Second simple gesture</param>
        /// <returns>Returns complex MouseGesture or MouseGesture.Unknown if no gesture is recognized.</returns>
        protected MouseGesture RecognizeComplexGeasture(MouseGesture firstGesture, MouseGesture secondGesture)
        {
            if (firstGesture == MouseGesture.Unknown || secondGesture == MouseGesture.Unknown)
                return MouseGesture.Unknown;

            //treats two simple gesture with the same direction with some unknown
            //segments between them as valid simple gesture
            //TODO consider disabling this
            if (firstGesture == secondGesture)
                return firstGesture;

            //see MouseGesture.cs for referecne how to compute complex gesture
            return (firstGesture | (MouseGesture)((int)secondGesture * 2));
        }

        /// <summary>
        /// Tries to recognize simple mouse gesture
        /// </summary>
        /// <param name="unknownBefore">The number of segments with SegmentDirection.Unknown before the gesture</param>
        /// <param name="length">The number of segments of the gesture.</param>
        /// <param name="unknownAfter">The number of segments with SegmentDirection.Unknown after the gesture</param>
        /// <param name="direction">The direction of the gesture.</param>
        /// <returns>Returns the simple gesture or MouseGesture.Unknown if no gesture is recognized.</returns>
        protected MouseGesture RecognizeSimpleGesture(int unknownBefore, int length, int unknownAfter, MouseMoveSegment.SegmentDirection direction)
        {
            // max length of unknown segments before and after gesture
            double lengthTolerance = length * _maxUnknownSkipRatio;
            // check unknown segments
            if ((unknownBefore < lengthTolerance) && (unknownAfter < lengthTolerance))
            {
                //according to the direction of the segment choose simple MouseGesture
                switch (direction)
                {
                    case MouseMoveSegment.SegmentDirection.Up:
                        return MouseGesture.Up;
                    case MouseMoveSegment.SegmentDirection.Right:
                        return MouseGesture.Right;
                    case MouseMoveSegment.SegmentDirection.Down:
                        return MouseGesture.Down;
                    case MouseMoveSegment.SegmentDirection.Left:
                        return MouseGesture.Left;
                }
            }

            return MouseGesture.Unknown;
        }
		// Private Methods (6) 

        /// <summary>
        /// Counts segments in row with SegmentDirection. 
        /// Counting started at the index of mouseMoveSegments array.
        /// </summary>
        /// <param name="index">
        /// Index to start at. Index to start next search is passed to
        /// this var.
        /// </param>
        /// <param name="segmentDirection">The direction of segments to count.</param>
        /// <returns>Returns the number of segments with direction in the row.</returns>
        private int CountMouseMoveSegments(ref int index, MouseMoveSegment.SegmentDirection segmentDirection)
        {
            int count = 0;

            while (index < _mouseMoveSegments.Count &&
                   _mouseMoveSegments[index].Direction == segmentDirection)
            {
                index++;
                count++;
            }

            return count;
        }

        /// <summary>
        /// Counts segments with the same direction in mouseMoveSegments.
        /// Counting started at the index of mouseMoveSegments array and
        /// the direction of segments is passed to the segmentDirection
        /// </summary>
        /// <param name="index">
        /// Index to start at. Index to start next search is passed to
        /// this var.
        /// </param>
        /// <param name="segmentDirection">
        /// The direction of the segments is passed
        /// to this var.
        /// </param>
        /// <returns>Returns the number of segments with the same direction in the row.</returns>
        private int CountMouseMoveSegments(ref int index, out MouseMoveSegment.SegmentDirection segmentDirection)
        {
            int count = 0;
            segmentDirection = MouseMoveSegment.SegmentDirection.Unknown;

            if (index < _mouseMoveSegments.Count)
            {
                segmentDirection = _mouseMoveSegments[index].Direction;
            }
            else
                return 0;

            while (index < _mouseMoveSegments.Count &&
              _mouseMoveSegments[index].Direction == segmentDirection)
            {
                index++;
                count++;
            }

            return count;
        }

        private void DoGestureRaised(MouseGestureEventArgs mouseGestureEventArgs)
        {
            if (GestureRaised != null)
            {
                GestureRaised(this, mouseGestureEventArgs);
            }
        }

        /// <summary>
        /// Calculates distance between 2 points
        /// </summary>
        /// <param name="p1">First point</param>
        /// <param name="p2">Second point</param>
        /// <returns>Distance between two points</returns>
        private static double GetDistance(Point p1, Point p2)
        {
            int dx = Convert.ToInt32(p1.X - p2.X);
            int dy = Convert.ToInt32(p1.Y - p2.Y);

            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Raises proper events
        /// </summary>
        /// <param name="gesture">Gesture performed.</param>
        private void RaiseGestureEvents(MouseGesture gesture)
        {
            if (gesture != MouseGesture.Unknown)
            {
                MouseGestureEventArgs eventArgs = new MouseGestureEventArgs(gesture, _gestureStartLocation);
                DoGestureRaised(eventArgs);
            }
        }

        /// <summary>
        /// Recognize gesture from the recorded data
        /// </summary>
        /// <returns>Returns MouseGesture or MouseGesture.Unknown if no gesture is recognized.</returns>
        /// <remarks>
        /// Funtion counts the number of unknown segments before the gestures,
        /// the number of segments in the gestures and the number of unknown segments
        /// after the gestures. These counts are keystone for the gesture recognition.
        /// </remarks>
        private MouseGesture RecognizeGesture()
        {
            int index = 0;

            int unknownSegmentsBefore = CountMouseMoveSegments(ref index, MouseMoveSegment.SegmentDirection.Unknown);

            MouseMoveSegment.SegmentDirection firstSegmentDirection;
            int firstGestureLenght = CountMouseMoveSegments(ref index, out firstSegmentDirection);


            int unknownSegmentsMiddle = CountMouseMoveSegments(ref index, MouseMoveSegment.SegmentDirection.Unknown);

            MouseMoveSegment.SegmentDirection secondSegmentDirection = MouseMoveSegment.SegmentDirection.Unknown;
            int secondGestureLength = 0;
            int unknownSegmentAfter = 0;

            //if complex gesture are enabled count segments for the second gesture
            if (EnableComplexGestures)
            {
                secondGestureLength = CountMouseMoveSegments(ref index, out secondSegmentDirection);

                unknownSegmentAfter = CountMouseMoveSegments(ref index, MouseMoveSegment.SegmentDirection.Unknown);
            }

            //if there are some segments left, the recorded data does not contain valid mouse gesture
            MouseMoveSegment.SegmentDirection nextSegment;
            if (CountMouseMoveSegments(ref index, out nextSegment) > 0)
            {
                return MouseGesture.Unknown;
            }

            //recognize firs gesture
            MouseGesture firstGesture =
              RecognizeSimpleGesture(unknownSegmentsBefore, firstGestureLenght,
                                     unknownSegmentsMiddle, firstSegmentDirection);

            //if complex gesture are enabled continue with second gesture
            MouseGesture secondGesture;
            if ((EnableComplexGestures) && (secondGestureLength > 0))
            {
                secondGesture = RecognizeSimpleGesture(unknownSegmentsMiddle, secondGestureLength,
                                                       unknownSegmentAfter, secondSegmentDirection);

                return RecognizeComplexGeasture(firstGesture, secondGesture);
            }
            else
                return firstGesture;
        }

		#endregion Methods 
    }
    
}
