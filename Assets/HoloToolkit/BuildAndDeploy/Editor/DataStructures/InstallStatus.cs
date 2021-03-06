// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace HoloToolkit.Unity
{
    [Serializable]
    public class InstallStatus
    {
        public int Code;
        public string CodeText;
        public string Reason;
        public bool Success;
    }
}