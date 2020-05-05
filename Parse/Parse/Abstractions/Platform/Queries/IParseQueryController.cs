// Copyright (c) 2015-present, Parse, LLC.  All rights reserved.  This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Parse.Abstractions.Platform.Objects;

namespace Parse.Abstractions.Platform.Queries
{
    public interface IParseQueryController
    {
        Task<IEnumerable<IObjectState>> FindAsync<T>(ParseQuery<T> query, ParseUser user, CancellationToken cancellationToken = default) where T : ParseObject;

        Task<int> CountAsync<T>(ParseQuery<T> query, ParseUser user, CancellationToken cancellationToken = default) where T : ParseObject;

        Task<IObjectState> FirstAsync<T>(ParseQuery<T> query, ParseUser user, CancellationToken cancellationToken = default) where T : ParseObject;
    }
}
