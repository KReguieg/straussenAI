using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class AI_Model : MonoBehaviour
{
    /// <summary>
    /// Enumerates the available chat models.
    /// </summary>
    public enum ChatModel
    {
        [Description("GPT 3.5 Turbo")]
        ChatGPT3,
        [Description("GPT 4")]
        ChatGPT4,
        [Description("GPT 4 32K")]
        ChatGPT432K,
        [Description("GPT 3.5 16K")]
        ChatGPT316K,
        [Description("GPT 4 Turbo")]
        ChatGPT4o
    }

    /// <summary>
    /// Enumerates the available audio models.
    /// </summary>
    public enum AudioModel
    {
        Whisper
    }

    /// <summary>
    /// Represents a model used for AI processing.
    /// </summary>
    public class Model
    {
        /// <summary>
        /// The name of the model.
        /// </summary>
        public string ModelName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Model"/> class.
        /// </summary>
        /// <param name="model">The name of the model.</param>
        public Model(string model)
        {
            ModelName = model;
        }

        /// <summary>
        /// Creates a new <see cref="Model"/> instance from the specified <see cref="ChatModel"/>.
        /// </summary>
        /// <param name="model">The <see cref="ChatModel"/> to create the model from.</param>
        /// <returns>A new <see cref="Model"/> instance.</returns>
        public static Model FromChatModel(ChatModel model)
        {
            return model switch
            {
                ChatModel.ChatGPT3 => new Model("gpt-3.5-turbo"),
                ChatModel.ChatGPT4 => new Model("gpt-4"),
                ChatModel.ChatGPT4o => new Model("gpt-4-turbo"),
                ChatModel.ChatGPT432K => new Model("gpt-4-32k"),
                ChatModel.ChatGPT316K => new Model("gpt-3.5-turbo-16k"),
                _ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
            };
        }

        public static Model FromAudioModel(AudioModel model)
        {
            return model switch
            {
                AudioModel.Whisper => new Model("whisper-1"),
                _ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
            };
        }
    }
}
