using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CS5643P2 {
    class Program : Game {
        GraphicsDeviceManager graphics;

        public Program() {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.ApplyChanges();
        }

        Cloth cloth;
        BasicEffect fx;
        Texture2D t;
        float a = 0;

        protected override void Initialize() {
            base.Initialize();
        }
        protected override void LoadContent() {
            cloth = new Cloth(GraphicsDevice, 100, 100, Vector2.One * 0.1f);

            int h = 17, w = h << 1;
            t = new Texture2D(GraphicsDevice, w, h);
            Color[] c = new Color[w * h];
            int i = 0;
            for(int x = 0; x < h; x++) {
                for(int y = 0; y < h; y++) {
                    c[y * w + x] = (i % 2 == 0) ? Color.Red : Color.Orange;
                    c[y * w + x + h] = (i % 2 == 0) ? Color.Blue : Color.Cyan;
                    i++;
                }
            }
            t.SetData(c);


            fx = new BasicEffect(GraphicsDevice);
            fx.TextureEnabled = true;
            fx.VertexColorEnabled = false;
            fx.LightingEnabled = false;
            fx.FogEnabled = false;
            fx.World = Matrix.Identity;
            fx.View = Matrix.CreateLookAt(new Vector3(12, 3, 12), Vector3.Zero, Vector3.Up);
            fx.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.01f, 1000f);
            fx.DiffuseColor = Vector3.One;
            fx.Texture = t;

            base.LoadContent();
        }
        protected override void UnloadContent() {
            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime) {
            a += (float)gameTime.ElapsedGameTime.TotalSeconds;
            a = MathHelper.WrapAngle(a);

            for(int z = 0; z < cloth.rows; z++) {
                for(int x = 0; x < cloth.stride; x++) {
                    Vector3 p = cloth[x, z];
                    cloth[x, z] = new Vector3(
                        p.X,
                        (float)(Math.Sin(p.X + a) * Math.Sin(p.Z + a)),
                        p.Z
                        );
                }
            }
            cloth.UpdatePositions();

            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Transparent);
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            cloth.SetBuffers(GraphicsDevice);
            fx.CurrentTechnique.Passes[0].Apply();
            cloth.Draw(GraphicsDevice);

            GraphicsDevice.SetVertexBuffers(null);
            GraphicsDevice.Indices = null;

            base.Draw(gameTime);
        }

        static void Main() {
            using(Program p = new Program()) {
                p.Run();
            }
        }
    }
}