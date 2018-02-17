using System;
using CocosSharp;

namespace GameForestMatch3.Views.Elements
{

    public class Button : CCNode
    {

        CCNode child;

        public event TriggeredHandler Triggered;
        // A delegate type for hooking up button triggered events
        public delegate void TriggeredHandler(object sender, EventArgs e);


        protected Button()
        {
            AttachListener();
        }

        public Button(CCSprite sprite)
            : this()
        {
            child = sprite;
            AddChild(sprite);
        }


        public Button(string text, int size = 0)
            : this()
        {
            var fontSize = size == 0 ? 60 : size;
            child = new CCLabel(text, "arial", fontSize);
            AddChild(child);

        }

        public string Text
        {
            get { return (child as CCLabel == null) ? string.Empty : ((CCLabel)child).Text; }
            set
            {
                if ((child as CCLabel) == null)
                    return;

                ((CCLabel)child).Text = value;
            }
        }

        void AttachListener()
        {
            // Register Touch Event
            var listener = new CCEventListenerTouchOneByOne();
            listener.IsSwallowTouches = true;

            listener.OnTouchBegan = OnTouchBegan;
            listener.OnTouchEnded = OnTouchEnded;
            listener.OnTouchCancelled = OnTouchCancelled;

            AddEventListener(listener, this);
        }

        bool touchHits(CCTouch touch)
        {
            var location = touch.Location;
            var area = child.BoundingBox;
            return area.ContainsPoint(child.WorldToParentspace(location));
        }

        bool OnTouchBegan(CCTouch touch, CCEvent touchEvent)
        {
            bool hits = touchHits(touch);
            if (hits)
                ScaleButtonTo(0.9f);

            return hits;
        }

        void OnTouchEnded(CCTouch touch, CCEvent touchEvent)
        {
            bool hits = touchHits(touch);
            if (hits && Triggered != null)
                Triggered(this, EventArgs.Empty);
            ScaleButtonTo(1);
        }

        void OnTouchCancelled(CCTouch touch, CCEvent touchEvent)
        {
            ScaleButtonTo(1);
        }

        void ScaleButtonTo(float scale)
        {
            var action = new CCScaleTo(0.1f, scale);
            action.Tag = 900;
            StopAction(900);
            RunAction(action);
        }
    }
}