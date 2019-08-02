﻿using Prism.Interactivity.InteractionRequest;

namespace Rml.Wpf.Interactivity.InteractionRequest
{
    /// <inheritdoc />
    /// <summary>
    /// </summary>
    public class Confirmation : INotification
    {
        /// <inheritdoc />
        public string Title { get; set; }

        /// <inheritdoc />
        public object Content { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public object[] Choices { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int ResultIndex { get; set; } = -1;
    }
}