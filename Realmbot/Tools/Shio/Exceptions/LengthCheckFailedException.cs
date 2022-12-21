using System;

namespace Stream.Exceptions {
    public class LengthCheckFailedException : Exception {
        public LengthCheckFailedException() {
        }

        public LengthCheckFailedException(string message) : base(message) {
        }
    }
}