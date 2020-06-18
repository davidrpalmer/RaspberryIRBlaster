using System;

namespace RaspberryIRBlaster.Client.BatchActions
{
    public interface IBatchAction<T> : IBatchAction
    {
        T Data { get; }
    }

    public interface IBatchAction // Allows referencing it without the generic param.
    {
        string Type { get; }
    }
}
