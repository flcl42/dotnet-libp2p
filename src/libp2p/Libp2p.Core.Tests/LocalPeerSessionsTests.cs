// SPDX-FileCopyrightText: 2026 Demerzel Solutions Limited
// SPDX-License-Identifier: MIT

using System.Collections;
using System.Collections.ObjectModel;
using Nethermind.Libp2p.Protocols;

namespace Nethermind.Libp2p.Core.Tests;

internal class LocalPeerSessionsTests
{
    [Test]
    public void InterfaceView_IsStableAndDoesNotExposeCollectionMutation()
    {
        LocalPeer peer = CreatePeer();
        ILocalPeer interfacePeer = peer;

        IReadOnlyCollection<ISession> sessions = interfacePeer.Sessions;

        Assert.Multiple(() =>
        {
            Assert.That(interfacePeer.Sessions, Is.SameAs(sessions));
            Assert.That(sessions, Is.Not.InstanceOf<ObservableCollection<LocalPeer.Session>>());
            Assert.That(sessions, Is.Not.InstanceOf<ICollection<LocalPeer.Session>>());
            Assert.That(sessions, Is.Not.InstanceOf<IList>());
        });
    }

    [Test]
    public void InterfaceView_EnumerationUsesPointInTimeSnapshot()
    {
        LocalPeer peer = CreatePeer();
        LocalPeer.Session firstSession = new(peer);
        LocalPeer.Session laterSession = new(peer);

        lock (peer.Sessions)
        {
            peer.Sessions.Add(firstSession);
        }

        IReadOnlyCollection<ISession> sessions = ((ILocalPeer)peer).Sessions;
        using IEnumerator<ISession> enumerator = sessions.GetEnumerator();

        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.SameAs(firstSession));

        lock (peer.Sessions)
        {
            peer.Sessions.Add(laterSession);
        }

        Assert.DoesNotThrow(() => Assert.That(enumerator.MoveNext(), Is.False));
        Assert.Multiple(() =>
        {
            Assert.That(sessions.Count, Is.EqualTo(2));
            Assert.That(sessions.ToArray(), Is.EqualTo(new ISession[] { firstSession, laterSession }));
        });
    }

    [Test]
    public async Task InterfaceView_EnumerationRemainsStableWhenSessionIsRemoved()
    {
        LocalPeer peer = CreatePeer();
        LocalPeer.Session removedSession = new(peer);
        LocalPeer.Session retainedSession = new(peer);

        lock (peer.Sessions)
        {
            peer.Sessions.Add(removedSession);
            peer.Sessions.Add(retainedSession);
        }

        IReadOnlyCollection<ISession> sessions = ((ILocalPeer)peer).Sessions;
        using IEnumerator<ISession> enumerator = sessions.GetEnumerator();

        await removedSession.DisconnectAsync();

        Assert.Multiple(() =>
        {
            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current, Is.SameAs(removedSession));
            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current, Is.SameAs(retainedSession));
            Assert.That(enumerator.MoveNext(), Is.False);
            Assert.That(sessions.Single(), Is.SameAs(retainedSession));
        });
    }

    [Test]
    public void IdentifyNotifier_UsesSynchronizedSessionView()
    {
        LocalPeer peer = CreatePeer();
        new IdentifyNotifier().TrackChanges(peer);
        using ManualResetEventSlim notificationStarted = new();
        Task notificationTask;

        lock (peer.Sessions)
        {
            notificationTask = Task.Run(() =>
            {
                notificationStarted.Set();
                peer.ListenAddresses.Add("/ip4/127.0.0.1/tcp/0");
            });

            Assert.That(notificationStarted.Wait(TimeSpan.FromSeconds(2)), Is.True);
            Assert.That(notificationTask.Wait(TimeSpan.FromMilliseconds(100)), Is.False);
        }

        Assert.That(notificationTask.Wait(TimeSpan.FromSeconds(2)), Is.True);
    }

    private static LocalPeer CreatePeer()
        => new(new Identity(Enumerable.Repeat((byte)42, 32).ToArray()), null, new ProtocolStackSettings());
}
