// Simple IPC test
import ipc from 'ipc:demo';

console.log('🚀 Simple IPC test...');

async function test() {
    try {
        console.log('1. Connecting...');
        const connected = await ipc.connect();
        console.log('Connected:', connected);
        
        if (!connected) {
            console.error('Failed to connect');
            return;
        }
        
        console.log('2. Getting users...');
        const users = await ipc.call('getusers');
        console.log('Users:', users);
        
        await ipc.disconnect();
        console.log('✅ Simple test done');
        
    } catch (error) {
        console.error('❌ Error:', error.message);
        console.error('Stack:', error.stack);
    }
}

test().catch(console.error);