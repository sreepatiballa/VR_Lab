tcpip = StartServer();
[vertString, edgeString, forceString] = ReadFromUnity(tcpip);
fprintf('%s\n', vertString);
fprintf('%s\n', edgeString);
fprintf('%s\n', forceString);


% vertString = '1(1.0000, 0.0000, 0.0000)I;2(0.0000, 1.0000, 1.0000)I;3(0.7000, 0.5500, 0.4000)O;4(1.0000, 0.5500, 0.4000)F;5(1.0000, 0.0000, 0.0000)T;6(1.0000, 0.0000, 0.0000)F;';
% edgeString = '1 3 3 2 3 4 ';
% forceString = '4(1.0000, 0.0000, 0.0000);2(0.0000, 1.0000, 1.0000)%

[node, I, O, F, T] = getNodeCoordArray(vertString);
elem = getElementArray(edgeString);
force = getForceArray(forceString);
%[ u ] = mat_unity( node, I, O, F, T, elem);%

%Can comment out the line below once the returned u is working correctly%
%Basically this is just testing code%
node(:,2:size(node,2)) = node(:,2:size(node,2)) + 0.1;

%Again, use u here instead of node once everything is working%
retString = getDeformedCoordString(node);
fwrite(tcpip,retString);
fwrite(tcpip,'|');
fwrite(tcpip, edgeString);
fclose(tcpip);

